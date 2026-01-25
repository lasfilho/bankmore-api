extern alias TransfApi;

using System.Net.Http;
using BankMore.Transferencia.Application.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Transferencia.Tests;

public sealed class CustomWebApplicationFactoryTransferencia : WebApplicationFactory<TransfApi::Program>
{
    private readonly Uri _contaBaseAddress;
    private readonly HttpMessageHandler _contaHandler;

    public CustomWebApplicationFactoryTransferencia(Uri contaBaseAddress, HttpMessageHandler contaHandler)
    {
        _contaBaseAddress = contaBaseAddress;
        _contaHandler = contaHandler;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"transferencia-test-{Guid.NewGuid()}.db");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath};Cache=Shared"
            });
        });

        // ✅ O pulo do gato: sobrescreve o ContaCorrenteClient para usar o TestServer handler
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(sp =>
            {
                var http = new HttpClient(_contaHandler, disposeHandler: false)
                {
                    BaseAddress = _contaBaseAddress
                };

                return new ContaCorrenteClient(http);
            });
        });
    }
}
