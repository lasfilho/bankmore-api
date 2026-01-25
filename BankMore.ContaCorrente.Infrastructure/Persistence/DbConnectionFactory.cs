using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace BankMore.ContaCorrente.Infrastructure.Persistence;

public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection string não configurada.");

        var builder = new SqliteConnectionStringBuilder(cs);

        if (!string.IsNullOrWhiteSpace(builder.DataSource) && builder.DataSource != ":memory:")
        {
            var fullPath = Path.IsPathRooted(builder.DataSource)
                ? builder.DataSource
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, builder.DataSource));

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            builder.DataSource = fullPath;
        }

        _connectionString = builder.ToString();
    }

    public DbConnection CreateConnection()
        => new SqliteConnection(_connectionString);
}
