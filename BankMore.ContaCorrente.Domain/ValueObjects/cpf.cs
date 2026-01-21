using System.Text.RegularExpressions;

namespace BankMore.ContaCorrente.Domain.ValueObjects
{
    public sealed class Cpf
    {
        public string Numero { get; }

        protected Cpf() { }

        public Cpf(string numero)
        {
            var normalizado = Normalizar(numero);

            if (!EhValido(normalizado))
                throw new ArgumentException("CPF inválido");

            Numero = normalizado;
        }

        private static string Normalizar(string cpf)
        {
            if (cpf is null) return string.Empty;
            return Regex.Replace(cpf, @"\D", "");
        }

        private static bool EhValido(string cpf)
        {
            if (cpf.Length != 11) return false;
            if (cpf.Distinct().Count() == 1) return false;

            int CalcDigito(ReadOnlySpan<char> baseCpf, int[] pesos)
            {
                var soma = 0;
                for (int i = 0; i < pesos.Length; i++)
                    soma += (baseCpf[i] - '0') * pesos[i];

                var resto = soma % 11;
                return resto < 2 ? 0 : 11 - resto;
            }

            var pesos1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var pesos2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var d1 = CalcDigito(cpf.AsSpan(0, 9), pesos1);
            if (cpf[9] - '0' != d1) return false;

            var d2 = CalcDigito(cpf.AsSpan(0, 10), pesos2);
            if (cpf[10] - '0' != d2) return false;

            return true;
        }
    }
}
