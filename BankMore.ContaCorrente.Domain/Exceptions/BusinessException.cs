namespace BankMore.ContaCorrente.Domain.Exceptions;

public sealed class BusinessException : Exception
{
    public string Type { get; }

    public BusinessException(string message, string type) : base(message)
        => Type = type;
}
