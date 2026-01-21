namespace BankMore.Transferencia.Domain.Exceptions;

public sealed class BusinessException : Exception
{
    public string TipoFalha { get; }

    public BusinessException(string mensagem, string tipoFalha)
        : base(mensagem)
    {
        TipoFalha = tipoFalha;
    }
}
