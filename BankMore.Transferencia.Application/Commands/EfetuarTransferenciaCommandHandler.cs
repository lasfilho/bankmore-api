using BankMore.Transferencia.Application.Clients;
using BankMore.Transferencia.Application.DTOs;
using BankMore.Transferencia.Domain.Exceptions;
using BankMore.Transferencia.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace BankMore.Transferencia.Application.Commands;

public sealed class EfetuarTransferenciaCommandHandler
{
    private readonly ITransferenciaRepository _repo;
    private readonly ContaCorrenteClient _contaClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EfetuarTransferenciaCommandHandler(
        ITransferenciaRepository repo,
        ContaCorrenteClient contaClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _contaClient = contaClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(EfetuarTransferenciaCommand command, CancellationToken ct = default)
    {
        // ---------- Token ----------
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("Token inválido ou expirado.");

        var authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Token inválido ou expirado.");

        var token = authHeader["Bearer ".Length..].Trim();

        // ---------- Validações ----------
        if (command.Valor <= 0)
            throw new BusinessException("Apenas valores positivos podem ser recebidos.", "INVALID_VALUE");

        if (string.IsNullOrWhiteSpace(command.NumeroContaDestino))
            throw new BusinessException("Número da conta de destino é obrigatório.", "INVALID_ACCOUNT");

        var contaDestino = command.NumeroContaDestino.Trim();

        // ---------- Idempotência mais segura ----------
        // Registra INICIADA primeiro. Se já existir (unique requestId), consideramos como idempotente e retornamos.
        try
        {
            await _repo.RegistrarAsync(command.RequestId, contaDestino, command.Valor, "INICIADA", DateTime.UtcNow);
        }
        catch
        {
            // Se sua implementação do repo lançar por UNIQUE, já foi processada/iniciada.
            // Você pode ser mais específico capturando exceção de SQLite (SqliteException) e checando constraint.
            return;
        }

        // 1) Débito na conta logada (NumeroConta null => Conta Corrente usa do token)
        var reqDebito = new MovimentarContaRequest(
            RequestId: command.RequestId,
            NumeroConta: null,
            Valor: command.Valor,
            Tipo: "D"
        );

        var debitoResp = await _contaClient.MovimentarAsync(reqDebito, token, ct);

        if (debitoResp.StatusCode == HttpStatusCode.Forbidden)
        {
            await _repo.AtualizarStatusAsync(command.RequestId, "FALHA_TOKEN_DEBITO", DateTime.UtcNow);
            throw new UnauthorizedAccessException("Token inválido ou expirado.");
        }

        if (!debitoResp.IsSuccessStatusCode)
        {
            var (msg, type) = await ReadErrorAsync(debitoResp, ct);
            await _repo.AtualizarStatusAsync(command.RequestId, "FALHA_DEBITO", DateTime.UtcNow);

            throw new BusinessException(
                $"Falha ao debitar conta origem. {msg}",
                type ?? "INVALID_ACCOUNT"
            );
        }

        // 2) Crédito na conta destino (enviar NumeroConta)
        var reqCredito = new MovimentarContaRequest(
            RequestId: command.RequestId,
            NumeroConta: contaDestino,
            Valor: command.Valor,
            Tipo: "C"
        );

        var creditoResp = await _contaClient.MovimentarAsync(reqCredito, token, ct);

        if (creditoResp.StatusCode == HttpStatusCode.Forbidden)
        {
            // Token morreu entre as chamadas => tentar estornar mesmo assim? Aqui vamos registrar e retornar 403.
            await _repo.AtualizarStatusAsync(command.RequestId, "FALHA_TOKEN_CREDITO", DateTime.UtcNow);
            throw new UnauthorizedAccessException("Token inválido ou expirado.");
        }

        if (!creditoResp.IsSuccessStatusCode)
        {
            // 3) Estorno (crédito na origem) se o crédito falhar
            // Use um requestId diferente para não colidir com idempotência do movimento original.
            var reqEstorno = new MovimentarContaRequest(
                RequestId: Guid.NewGuid(),
                NumeroConta: null,
                Valor: command.Valor,
                Tipo: "C"
            );

            var estornoResp = await _contaClient.MovimentarAsync(reqEstorno, token, ct);

            var (creditoMsg, creditoType) = await ReadErrorAsync(creditoResp, ct);

            if (!estornoResp.IsSuccessStatusCode)
            {
                var (estornoMsg, _) = await ReadErrorAsync(estornoResp, ct);
                await _repo.AtualizarStatusAsync(command.RequestId, "FALHA_CREDITO_ESTORNO_FALHOU", DateTime.UtcNow);

                throw new BusinessException(
                    $"Falha ao creditar conta destino e estorno falhou. Credito: {creditoMsg} | Estorno: {estornoMsg}",
                    creditoType ?? "INVALID_ACCOUNT"
                );
            }

            await _repo.AtualizarStatusAsync(command.RequestId, "FALHA_CREDITO_ESTORNADO", DateTime.UtcNow);

            throw new BusinessException(
                $"Falha ao creditar conta destino. Estorno executado. {creditoMsg}",
                creditoType ?? "INVALID_ACCOUNT"
            );
        }

        // 4) Sucesso
        await _repo.AtualizarStatusAsync(command.RequestId, "SUCESSO", DateTime.UtcNow);
    }

    private sealed record ApiError(string message, string type);

    private static async Task<(string message, string? type)> ReadErrorAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(body))
            return ($"HTTP {(int)resp.StatusCode} ({resp.StatusCode})", null);

        try
        {
            // Espera algo como: { "message": "...", "type": "INVALID_VALUE" }
            var err = JsonSerializer.Deserialize<ApiError>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (err != null && (!string.IsNullOrWhiteSpace(err.message) || !string.IsNullOrWhiteSpace(err.type)))
                return (err.message ?? body, err.type);
        }
        catch
        {
            // ignora parse
        }

        return (body, null);
    }
}
