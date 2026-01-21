using Dapper;
using Microsoft.Extensions.Logging;

namespace BankMore.Transferencia.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(DbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // 1) Caminho direto (Docker): /app/Database/scripts-transferencia.sql
        var directPath = Path.Combine(AppContext.BaseDirectory, "Database", "scripts-transferencia.sql");

        if (File.Exists(directPath))
        {
            var sql = await File.ReadAllTextAsync(directPath);

            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql);

            _logger.LogInformation("Banco Transferência inicializado via caminho direto: {Path}", directPath);
            return;
        }

        // 2) Fallback (local/VS): sobe diretórios até achar Database/scripts-transferencia.sql
        var baseDir = AppContext.BaseDirectory;

        var scriptPath = FindUpwards(baseDir, Path.Combine("Database", "scripts-transferencia.sql"));

        if (scriptPath is null)
            throw new FileNotFoundException("Não foi possível localizar Database/scripts-transferencia.sql a partir de " + baseDir);

        var sqlFallback = await File.ReadAllTextAsync(scriptPath);

        using var connFallback = _connectionFactory.CreateConnection();
        await connFallback.ExecuteAsync(sqlFallback);

        _logger.LogInformation("Banco Transferência inicializado via fallback: {Path}", scriptPath);
    }

    private static string? FindUpwards(string startDir, string relativeTarget)
    {
        var dir = new DirectoryInfo(startDir);

        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativeTarget);
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return null;
    }
}
