using BankMore.Transferencia.API.Security;
using BankMore.Transferencia.Application.Clients;
using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Application.DependencyInjection;
using BankMore.Transferencia.Infrastructure.DependencyInjection;
using BankMore.Transferencia.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

builder.Services.AddScoped<EfetuarTransferenciaCommandHandler>();

builder.Services.AddHttpClient<ContaCorrenteClient>(client =>
{
    var baseUrl = builder.Configuration["ContaCorrenteApi:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            JwtConfig.GetValidationParameters(builder.Configuration);
    });

builder.Services.AddAuthorization();

SQLitePCL.Batteries.Init();

var app = builder.Build();

// Inicializa o banco (Transferência)
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

