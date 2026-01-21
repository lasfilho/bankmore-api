using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Domain.Exceptions;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using ContaCorrenteEntity = BankMore.ContaCorrente.Domain.Entities.ContaCorrente;

namespace BankMore.ContaCorrente.Application.Commands
{
    public sealed class LoginContaCommandHandler
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IConfiguration _configuration;

        public LoginContaCommandHandler(IContaCorrenteRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<string> Handle(LoginContaCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Senha))
                throw new UnauthorizedBusinessException("Credenciais inválidas.", "USER_UNAUTHORIZED");

            var cpf = command.Cpf?.Trim();
            var numeroConta = command.NumeroConta?.Trim();

            if (string.IsNullOrWhiteSpace(cpf) && string.IsNullOrWhiteSpace(numeroConta))
                throw new UnauthorizedBusinessException("Informe CPF ou Número da Conta.", "USER_UNAUTHORIZED");

            ContaCorrenteEntity? conta =
                !string.IsNullOrWhiteSpace(cpf)
                    ? await _repository.ObterPorCpfAsync(cpf)
                    : await _repository.ObterPorNumeroAsync(numeroConta!);

            if (conta is null)
                throw new UnauthorizedBusinessException("Credenciais inválidas.", "USER_UNAUTHORIZED");

            var senhaValida = BCrypt.Net.BCrypt.Verify(command.Senha, conta.SenhaHash);
            if (!senhaValida)
                throw new UnauthorizedBusinessException("Credenciais inválidas.", "USER_UNAUTHORIZED");

            return JwtTokenGenerator.GerarToken(conta, _configuration);
        }
    }
}
