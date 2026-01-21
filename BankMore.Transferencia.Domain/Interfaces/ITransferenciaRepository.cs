namespace BankMore.Transferencia.Domain.Interfaces;

public interface ITransferenciaRepository
{
    Task RegistrarAsync(Guid requestId, string contaDestino, decimal valor, string status, DateTime dataHora);
    Task AtualizarStatusAsync(Guid requestId, string status, DateTime dataHora);
    Task<bool> JaProcessadaAsync(Guid requestId); // opcional com o fluxo acima
}
