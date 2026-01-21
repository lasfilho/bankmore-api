namespace BankMore.Transferencia.Application.DTOs;

public sealed record MovimentarContaRequest(
    Guid RequestId,
    string? NumeroConta,
    decimal Valor,
    string Tipo // "C" ou "D"
);
