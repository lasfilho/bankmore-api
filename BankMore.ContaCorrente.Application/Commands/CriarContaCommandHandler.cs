using ContaCorrenteEntity = BankMore.ContaCorrente.Domain.Entities.ContaCorrente;
using BankMore.ContaCorrente.Domain.Exceptions;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Application.Commands
{
    public class CriarContaCommandHandler
    {
        private readonly IContaCorrenteRepository _repository;

        public CriarContaCommandHandler(IContaCorrenteRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(CriarContaCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Cpf))
                throw new BusinessException("CPF é obrigatório.", "INVALID_DOCUMENT");

            if (string.IsNullOrWhiteSpace(command.Senha))
                throw new BusinessException("Senha é obrigatória.", "INVALID_DOCUMENT");

            if (string.IsNullOrWhiteSpace(command.NomeTitular))
                throw new BusinessException("Nome do titular é obrigatório.", "INVALID_DOCUMENT");

            // 🔹 Validação de CPF via ValueObject
            string cpfNormalizado;
            try
            {
                cpfNormalizado = new Cpf(command.Cpf).Numero;
            }
            catch (ArgumentException)
            {
                throw new BusinessException("CPF inválido.", "INVALID_DOCUMENT");
            }

            // 🔹 CPF duplicado
            var contaExistente = await _repository.ObterPorCpfAsync(cpfNormalizado);
            if (contaExistente != null)
                throw new BusinessException("CPF já cadastrado.", "INVALID_DOCUMENT");

            // 🔹 Hash da senha
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(command.Senha);

            // =====================================================
            // 🔥 AQUI É O TRECHO QUE VOCÊ SUBSTITUI
            // =====================================================

            ContaCorrenteEntity conta;

            for (int tentativa = 0; tentativa < 10; tentativa++)
            {
                conta = new ContaCorrenteEntity(
                    command.NomeTitular.Trim(),
                    cpfNormalizado,
                    senhaHash
                );

                // Verifica se o número da conta já existe
                var existe = await _repository.ObterPorNumeroAsync(conta.NumeroConta);

                if (existe is null)
                {
                    await _repository.AdicionarAsync(conta);
                    return conta.NumeroConta;
                }
            }

            // Se após 10 tentativas não conseguiu gerar número único
            throw new BusinessException(
                "Não foi possível gerar um número de conta válido. Tente novamente.",
                "INVALID_ACCOUNT"
            );
        }
    }
}
