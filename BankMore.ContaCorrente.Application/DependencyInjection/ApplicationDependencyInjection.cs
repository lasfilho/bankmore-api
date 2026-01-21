using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.ContaCorrente.Application.DependencyInjection
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CriarContaCommandHandler>();
            services.AddScoped<LoginContaCommandHandler>();
            services.AddScoped<InativarContaCommandHandler>();
            services.AddScoped<MovimentarContaCommandHandler>();
            services.AddScoped<SaldoContaQueryHandler>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
