namespace BankMore.ContaCorrente.Domain.Interfaces
{
    public interface IContaCorrenteRepository
    {
        Task<Entities.ContaCorrente?> ObterPorCpfAsync(string cpf);
        Task<Entities.ContaCorrente?> ObterPorNumeroAsync(string numeroConta);
        Task AdicionarAsync(Entities.ContaCorrente conta);
        Task AtualizarAsync(Entities.ContaCorrente conta);
        Task<bool> MovimentoJaProcessadoAsync(Guid requestId, string numeroContaAlvo);
        Task RegistrarMovimentoAsync(Guid requestId, string numeroConta, decimal valor, string tipo, DateTime dataHora);
        Task<decimal> CalcularSaldoAsync(string numeroConta);
        Task<bool> NumeroContaExisteAsync(string numeroConta);


    }
}
