using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Persistence;
using BankMore.Transferencia.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Transferencia.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<DbConnectionFactory>();
            services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

            services.AddSingleton<DatabaseInitializer>();

            return services;
        }
    }
}
