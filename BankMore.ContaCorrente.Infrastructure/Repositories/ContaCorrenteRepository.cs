using ContaCorrenteEntity = BankMore.ContaCorrente.Domain.Entities.ContaCorrente;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using Dapper;
using System.Globalization;

namespace BankMore.ContaCorrente.Infrastructure.Repositories
{
    public sealed class ContaCorrenteRepository : IContaCorrenteRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public ContaCorrenteRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private sealed record ContaCorrenteRow(
            string Id,
            string NumeroConta,
            string NomeTitular,
            string Cpf,
            string SenhaHash,
            long Ativo,
            string DataCriacao
        );

        private static ContaCorrenteEntity Map(ContaCorrenteRow row)
        {
            return new ContaCorrenteEntity(
                id: Guid.Parse(row.Id),
                numeroConta: row.NumeroConta,
                nomeTitular: row.NomeTitular,
                cpf: row.Cpf,
                senhaHash: row.SenhaHash,
                ativo: row.Ativo == 1,
                dataCriacao: DateTime.Parse(row.DataCriacao, null, DateTimeStyles.RoundtripKind)
            );
        }

        public async Task<ContaCorrenteEntity?> ObterPorCpfAsync(string cpf)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    ID           AS Id,
                    NUMERO_CONTA AS NumeroConta,
                    NOME_TITULAR AS NomeTitular,
                    CPF          AS Cpf,
                    SENHA_HASH   AS SenhaHash,
                    ATIVO        AS Ativo,
                    DATA_CRIACAO AS DataCriacao
                FROM CONTACORRENTE
                WHERE CPF = @cpf
            """;

            var row = await conn.QueryFirstOrDefaultAsync<ContaCorrenteRow>(sql, new { cpf });
            return row is null ? null : Map(row);
        }

        public async Task<ContaCorrenteEntity?> ObterPorNumeroAsync(string numeroConta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    ID           AS Id,
                    NUMERO_CONTA AS NumeroConta,
                    NOME_TITULAR AS NomeTitular,
                    CPF          AS Cpf,
                    SENHA_HASH   AS SenhaHash,
                    ATIVO        AS Ativo,
                    DATA_CRIACAO AS DataCriacao
                FROM CONTACORRENTE
                WHERE NUMERO_CONTA = @numeroConta
            """;

            var row = await conn.QueryFirstOrDefaultAsync<ContaCorrenteRow>(sql, new { numeroConta });
            return row is null ? null : Map(row);
        }

        public async Task AdicionarAsync(ContaCorrenteEntity conta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                INSERT INTO CONTACORRENTE
                (ID, NUMERO_CONTA, NOME_TITULAR, CPF, SENHA_HASH, ATIVO, DATA_CRIACAO)
                VALUES
                (@Id, @NumeroConta, @NomeTitular, @Cpf, @SenhaHash, @Ativo, @DataCriacao)
            """;

            await conn.ExecuteAsync(sql, new
            {
                Id = conta.Id.ToString(),
                conta.NumeroConta,
                conta.NomeTitular,
                conta.Cpf,
                conta.SenhaHash,
                Ativo = conta.Ativo ? 1 : 0,
                DataCriacao = conta.DataCriacao.ToString("O")
            });
        }

        public async Task AtualizarAsync(ContaCorrenteEntity conta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                UPDATE CONTACORRENTE
                SET ATIVO = @Ativo
                WHERE ID = @Id
            """;

            await conn.ExecuteAsync(sql, new
            {
                Id = conta.Id.ToString(),
                Ativo = conta.Ativo ? 1 : 0
            });
        }

        // ---------------- MOVIMENTAÇÃO ----------------

        public async Task<bool> MovimentoJaProcessadoAsync(Guid requestId, string numeroConta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                                    SELECT 1 FROM MOVIMENTO
                                    WHERE REQUEST_ID = @requestId
                                      AND NUMERO_CONTA = @numeroConta
                                    LIMIT 1
                                """;

            var result = await conn.ExecuteScalarAsync<int?>(sql, new
            {
                requestId = requestId.ToString(),
                numeroConta
            });

            return result.HasValue;
        }


        public async Task RegistrarMovimentoAsync(
            Guid requestId,
            string numeroConta,
            decimal valor,
            string tipo,
            DateTime dataHora)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                INSERT INTO MOVIMENTO
                (REQUEST_ID, NUMERO_CONTA, VALOR, TIPO, DATA_HORA)
                VALUES
                (@requestId, @numeroConta, @valor, @tipo, @dataHora)
            """;

            await conn.ExecuteAsync(sql, new
            {
                requestId = requestId.ToString(),
                numeroConta,
                valor,
                tipo,
                dataHora = dataHora.ToString("O")
            });
        }

        public async Task<decimal> CalcularSaldoAsync(string numeroConta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    COALESCE(SUM(CASE WHEN TIPO = 'C' THEN VALOR ELSE 0 END), 0)
                  - COALESCE(SUM(CASE WHEN TIPO = 'D' THEN VALOR ELSE 0 END), 0)
                FROM MOVIMENTO
                WHERE NUMERO_CONTA = @numeroConta
            """;

            return await conn.ExecuteScalarAsync<decimal>(sql, new { numeroConta });
        }

        public async Task<bool> NumeroContaExisteAsync(string numeroConta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT 1 FROM CONTACORRENTE
                WHERE NUMERO_CONTA = @numeroConta
                LIMIT 1
            """;

            var result = await conn.ExecuteScalarAsync<int?>(sql, new { numeroConta });
            return result.HasValue;
        }

        public Task<bool> MovimentoJaProcessadoAsync(Guid requestId)
        {
            throw new NotImplementedException();
        }
    }
}
