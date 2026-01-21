namespace BankMore.ContaCorrente.Application.Queries;

public sealed record SaldoContaResponse(
    string NumeroConta,
    string NomeTitular,
    DateTime DataHoraConsulta,
    decimal Saldo
);
