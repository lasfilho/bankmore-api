using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Domain.Exceptions;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BankMore.ContaCorrente.Application.Queries;

public sealed class SaldoContaQueryHandler
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SaldoContaQueryHandler(IContaCorrenteRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SaldoContaResponse> Handle()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity!.IsAuthenticated)
            throw new UnauthorizedAccessException("Token inválido ou expirado.");

        var numeroConta = user.FindFirst(JwtClaims.NumeroConta)?.Value;

        if (string.IsNullOrWhiteSpace(numeroConta))
            throw new BusinessException("Conta corrente inválida.", "INVALID_ACCOUNT");

        var conta = await _repository.ObterPorNumeroAsync(numeroConta);
        if (conta == null)
            throw new BusinessException("Conta corrente inválida.", "INVALID_ACCOUNT");

        if (!conta.Ativo)
            throw new BusinessException("Conta corrente inativa.", "INACTIVE_ACCOUNT");

        var saldo = await _repository.CalcularSaldoAsync(numeroConta);

        return new SaldoContaResponse(
            NumeroConta: conta.NumeroConta,
            NomeTitular: conta.NomeTitular,
            DataHoraConsulta: DateTime.UtcNow,
            Saldo: saldo
        );
    }
}
