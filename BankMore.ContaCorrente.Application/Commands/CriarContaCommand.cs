namespace BankMore.ContaCorrente.Application.Commands
{
    public class CriarContaCommand
    {
        public string NomeTitular { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}
