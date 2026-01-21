using Dapper;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BankMore.ContaCorrente.Infrastructure.Persistence;

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
        // =====================================================
        // 1️⃣ PRIMEIRO: tenta o caminho direto no container
        // /app/Database/scripts.sql
        // =====================================================
        var directPath = Path.Combine(
            AppContext.BaseDirectory,
            "Database",
            "scripts.sql"
        );

        if (File.Exists(directPath))
        {
            var sql = await File.ReadAllTextAsync(directPath);

            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql);

            return;
        }

        // =====================================================
        // 2️⃣ FALLBACK: sobe diretórios (execução local / VS)
        // =====================================================
        var baseDir = AppContext.BaseDirectory;

        var scriptPath = FindUpwards(
            baseDir,
            Path.Combine("Database", "scripts.sql")
        );

        if (scriptPath is null)
            throw new FileNotFoundException(
                "Não foi possível localizar Database/scripts.sql"
            );

        var sqlFallback = await File.ReadAllTextAsync(scriptPath);

        using var connFallback = _connectionFactory.CreateConnection();
        await connFallback.ExecuteAsync(sqlFallback);
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
