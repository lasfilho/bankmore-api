namespace BankMore.ContaCorrente.Application.Commands
{
    public class LoginContaCommand
    {
        public string? Cpf { get; set; }
        public string? NumeroConta { get; set; }
        public string Senha { get; set; } = null!;
    }
}
