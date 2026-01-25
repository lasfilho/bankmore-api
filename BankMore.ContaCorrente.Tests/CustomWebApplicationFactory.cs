using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dbPath = Path.Combine(
                Path.GetTempPath(),
                $"contacorrente-test-{Guid.NewGuid()}.db");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"]
                    = $"Data Source={dbPath};Cache=Shared"
            });
        });

    }
}
