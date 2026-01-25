using BankMore.ContaCorrente.API.Middlewares;
using BankMore.ContaCorrente.API.Security;
using BankMore.ContaCorrente.Application.DependencyInjection;
using BankMore.ContaCorrente.Infrastructure.DependencyInjection;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;




var builder = WebApplication.CreateBuilder(args);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations(); 
});


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            JwtConfig.GetValidationParameters(builder.Configuration);

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8";

                var payload = JsonSerializer.Serialize(new
                {
                    message = "Token inválido ou expirado.",
                    type = "USER_UNAUTHORIZED"
                });

                return context.Response.WriteAsync(payload);
            },

            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8"; 

                var payload = JsonSerializer.Serialize(new
                {
                    message = "Token inválido ou expirado.",
                    type = "USER_UNAUTHORIZED"
                });

                return context.Response.WriteAsync(payload);
            }
        };
    });


builder.Services.AddAuthorization();

SQLitePCL.Batteries.Init();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

