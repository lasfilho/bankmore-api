using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Domain.Exceptions;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BankMore.ContaCorrente.Application.Commands;

public sealed class MovimentarContaCommandHandler
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MovimentarContaCommandHandler(
        IContaCorrenteRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(MovimentarContaCommand command)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity!.IsAuthenticated)
            throw new UnauthorizedAccessException("Token inválido ou expirado.");

        var numeroContaLogada = user.FindFirst(JwtClaims.NumeroConta)?.Value;



        if (string.IsNullOrWhiteSpace(numeroContaLogada))
            throw new BusinessException("Conta do token inválida.", "INVALID_ACCOUNT");

        if (command.Valor <= 0)
            throw new BusinessException("Apenas valores positivos podem ser recebidos.", "INVALID_VALUE");

        var tipo = (command.Tipo ?? "").Trim().ToUpperInvariant();

        if (tipo is not ("C" or "D"))
            throw new BusinessException("Apenas os tipos 'C' (Crédito) ou 'D' (Débito) podem ser aceitos.", "INVALID_TYPE");

        var numeroContaAlvo = string.IsNullOrWhiteSpace(command.NumeroConta)
            ? numeroContaLogada
            : command.NumeroConta.Trim();

        Console.WriteLine($"[MOV] tokenConta={numeroContaLogada} cmd.NumeroConta={command.NumeroConta} tipo={tipo} valor={command.Valor} requestId={command.RequestId}");
        Console.WriteLine($"[MOV] numeroContaAlvo={numeroContaAlvo}");

        // Idempotência por (RequestId + Conta)  
        if (await _repository.MovimentoJaProcessadoAsync(command.RequestId, numeroContaAlvo))
            return;

        // Conta diferente da logada => apenas crédito
        if (!string.Equals(numeroContaAlvo, numeroContaLogada, StringComparison.OrdinalIgnoreCase) && tipo != "C")
            throw new BusinessException("Apenas crédito é permitido ao movimentar outra conta.", "INVALID_TYPE");

        var conta = await _repository.ObterPorNumeroAsync(numeroContaAlvo);
        if (conta == null)
            throw new BusinessException("Conta corrente inválida.", "INVALID_ACCOUNT");

        if (!conta.Ativo)
            throw new BusinessException("Conta corrente inativa.", "INACTIVE_ACCOUNT");

        await _repository.RegistrarMovimentoAsync(
            requestId: command.RequestId,
            numeroConta: numeroContaAlvo,
            valor: command.Valor,
            tipo: tipo,
            dataHora: DateTime.UtcNow
        );
    }
}
