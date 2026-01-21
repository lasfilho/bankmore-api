using BankMore.ContaCorrente.API.Middlewares;
using BankMore.ContaCorrente.API.Security;
using BankMore.ContaCorrente.Application.DependencyInjection;
using BankMore.ContaCorrente.Infrastructure.DependencyInjection;
using BankMore.ContaCorrente.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;


builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            JwtConfig.GetValidationParameters(builder.Configuration);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Inicializa o banco
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
