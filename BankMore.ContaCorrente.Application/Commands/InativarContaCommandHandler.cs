using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Domain.Exceptions;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BankMore.ContaCorrente.Application.Commands
{
    public class InativarContaCommandHandler
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InativarContaCommandHandler(
            IContaCorrenteRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Handle(InativarContaCommand command)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("Token inválido ou expirado.");

            var numeroConta = user.FindFirst(JwtClaims.NumeroConta)?.Value;

            if (string.IsNullOrEmpty(numeroConta))
                throw new BusinessException("Conta corrente inválida.", "INVALID_ACCOUNT");

            var conta = await _repository.ObterPorNumeroAsync(numeroConta);

            if (conta == null)
                throw new BusinessException("Conta corrente inválida.", "INVALID_ACCOUNT");

            var senhaValida = BCrypt.Net.BCrypt.Verify(command.Senha, conta.SenhaHash);

            if (!senhaValida)
                throw new BusinessException("Senha inválida.", "USER_UNAUTHORIZED");

            conta.Inativar();
            await _repository.AtualizarAsync(conta);
        }
    }
}
