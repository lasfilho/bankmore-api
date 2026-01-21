using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Persistence;
using Dapper;

namespace BankMore.Transferencia.Infrastructure.Repositories;

public sealed class TransferenciaRepository : ITransferenciaRepository
{
    private readonly DbConnectionFactory _factory;

    public TransferenciaRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task AtualizarStatusAsync(Guid requestId, string status, DateTime dataHora)
    {
        using var conn = _factory.CreateConnection();

        const string sql = """
            UPDATE TRANSFERENCIA
               SET STATUS = @status,
                   DATA_HORA = @dataHora
             WHERE REQUEST_ID = @requestId
        """;

        await conn.ExecuteAsync(sql, new
        {
            requestId = requestId.ToString(),
            status,
            dataHora = dataHora.ToString("O")
        });
    }

    public async Task<bool> JaProcessadaAsync(Guid requestId)
    {
        using var conn = _factory.CreateConnection();

        const string sql = """
            SELECT 1 FROM TRANSFERENCIA
             WHERE REQUEST_ID = @requestId
             LIMIT 1
        """;

        var r = await conn.ExecuteScalarAsync<int?>(sql, new { requestId = requestId.ToString() });
        return r.HasValue;
    }

    public async Task RegistrarAsync(Guid requestId, string contaDestino, decimal valor, string status, DateTime dataHora)
    {
        using var conn = _factory.CreateConnection();

        const string sql = """
            INSERT INTO TRANSFERENCIA (REQUEST_ID, CONTA_DESTINO, VALOR, STATUS, DATA_HORA)
            VALUES (@requestId, @contaDestino, @valor, @status, @dataHora)
        """;

        await conn.ExecuteAsync(sql, new
        {
            requestId = requestId.ToString(),
            contaDestino,
            valor,
            status,
            dataHora = dataHora.ToString("O")
        });
    }
}
