namespace BankMore.ContaCorrente.Application.Commands;

public sealed record MovimentarContaCommand(
    Guid RequestId,
    string? NumeroConta,
    decimal Valor,
    string Tipo // "C" ou "D"
);
