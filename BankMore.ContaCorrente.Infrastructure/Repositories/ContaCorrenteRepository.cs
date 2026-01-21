using ContaCorrenteEntity = BankMore.ContaCorrente.Domain.Entities.ContaCorrente;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using Dapper;

namespace BankMore.ContaCorrente.Infrastructure.Repositories
{
    public sealed class ContaCorrenteRepository : IContaCorrenteRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public ContaCorrenteRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<ContaCorrenteEntity?> ObterPorCpfAsync(string cpf)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT * FROM CONTACORRENTE
                WHERE CPF = @cpf
            """;

            return await conn.QueryFirstOrDefaultAsync<ContaCorrenteEntity>(sql, new { cpf });
        }

        public async Task<ContaCorrenteEntity?> ObterPorNumeroAsync(string numeroConta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT * FROM CONTACORRENTE
                WHERE NUMERO_CONTA = @numeroConta
            """;

            return await conn.QueryFirstOrDefaultAsync<ContaCorrenteEntity>(sql, new { numeroConta });
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

            await conn.ExecuteAsync(sql, conta);
        }

        public async Task AtualizarAsync(ContaCorrenteEntity conta)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                UPDATE CONTACORRENTE
                SET ATIVO = @Ativo
                WHERE ID = @Id
            """;

            await conn.ExecuteAsync(sql, conta);
        }

        // ---------------- MOVIMENTAÇÃO ----------------

        public async Task<bool> MovimentoJaProcessadoAsync(Guid requestId)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT 1 FROM MOVIMENTO
                WHERE REQUEST_ID = @requestId
                LIMIT 1
            """;

            var result = await conn.ExecuteScalarAsync<int?>(sql, new { requestId = requestId.ToString() });
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
                dataHora
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

    }
}
