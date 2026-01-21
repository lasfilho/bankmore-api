namespace BankMore.ContaCorrente.Domain.Exceptions;

public sealed class UnauthorizedBusinessException : Exception
{
    public string Type { get; }

    public UnauthorizedBusinessException(string message, string type) : base(message)
        => Type = type;
}
