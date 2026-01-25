extern alias ContaApi;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BankMore.Transferencia.Tests;

public sealed class ContaCorrenteFactory : WebApplicationFactory<ContaApi::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"contacorrente-test-{Guid.NewGuid()}.db");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath};Cache=Shared"
            });
        });
    }
}
