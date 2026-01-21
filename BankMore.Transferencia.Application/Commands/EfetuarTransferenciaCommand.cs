namespace BankMore.Transferencia.Application.Commands;

public sealed record EfetuarTransferenciaCommand(
    Guid RequestId,
    string NumeroContaDestino,
    decimal Valor
);
