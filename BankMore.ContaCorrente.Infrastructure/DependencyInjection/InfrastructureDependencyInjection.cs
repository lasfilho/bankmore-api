using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.ContaCorrente.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<DbConnectionFactory>();
            services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();

            services.AddSingleton<DatabaseInitializer>();

            return services;
        }
    }
}
