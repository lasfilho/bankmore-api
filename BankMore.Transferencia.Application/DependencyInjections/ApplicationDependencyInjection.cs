using BankMore.Transferencia.Application.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Transferencia.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Handlers
        services.AddHttpClient<ContaCorrenteClient>(client =>
        {
            var baseUrl = configuration["ContaCorrenteApi:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Config 'ContaCorrenteApi:BaseUrl' não encontrada no appsettings.");

            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        });


        return services;
    }
}
