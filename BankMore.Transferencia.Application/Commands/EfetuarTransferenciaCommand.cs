namespace BankMore.Transferencia.Application.Commands;

public sealed class EfetuarTransferenciaCommand
{
    public Guid RequestId { get; set; }
    public string NumeroContaDestino { get; set; } = null!;
    public decimal Valor { get; set; }
}
