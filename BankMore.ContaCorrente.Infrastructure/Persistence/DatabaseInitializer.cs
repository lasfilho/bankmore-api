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
        // Caminho relativo: raiz da aplicação (bin) -> volta pra raiz da solution
        // Estrutura esperada:
        // <solution>/Database/scripts.sql
        var baseDir = AppContext.BaseDirectory;

        // Sobe pastas até achar "Database/scripts.sql"
        // (funciona bem rodando via Visual Studio e em container, desde que você copie o arquivo)
        var scriptPath = FindUpwards(baseDir, Path.Combine("Database", "scripts.sql"));

        if (scriptPath is null)
            throw new FileNotFoundException("Não foi possível localizar Database/scripts.sql a partir de " + baseDir);

        var sql = await File.ReadAllTextAsync(scriptPath);

        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync(sql);

        _logger.LogInformation("Banco inicializado com script: {ScriptPath}", scriptPath);
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
