using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Domain.Entities
{
    public class ContaCorrente
    {
        public Guid Id { get; private set; }
        public string NumeroConta { get; private set; }
        public string NomeTitular { get; private set; }
        public string Cpf { get; private set; }
        public string SenhaHash { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime DataCriacao { get; private set; }

        public ContaCorrente(Guid id,
                                string numeroConta,
                                string nomeTitular,
                                string cpf,
                                string senhaHash,
                                bool ativo,
                                DateTime dataCriacao)
        {
            Id = id;
            NumeroConta = numeroConta;
            NomeTitular = nomeTitular;
            Cpf = cpf;
            SenhaHash = senhaHash;
            Ativo = ativo;
            DataCriacao = dataCriacao;
        }


        protected ContaCorrente() { } // Necessário para ORM

        public ContaCorrente(string nomeTitular, string cpf, string senhaHash)
        {
            Id = Guid.NewGuid();
            NumeroConta = GerarNumeroConta();
            NomeTitular = nomeTitular;
            Cpf = cpf;
            SenhaHash = senhaHash;
            Ativo = true;
            DataCriacao = DateTime.UtcNow;
        }

        public void Inativar()
        {
            Ativo = false;
        }

        private string GerarNumeroConta()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}

