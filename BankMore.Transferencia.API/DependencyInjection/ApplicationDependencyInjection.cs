using BankMore.Transferencia.Application.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Transferencia.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<ContaCorrenteClient>(client =>
        {
            var baseUrl = configuration["ContaCorrenteApi:BaseUrl"];
            client.BaseAddress = new Uri(baseUrl!);
        });

        return services;
    }
}
