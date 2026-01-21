using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BankMore.Transferencia.API.Security
{
    public static class JwtConfig
    {
        public static TokenValidationParameters GetValidationParameters(IConfiguration configuration)
        {
            var secret = configuration["Jwt:Secret"];
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = issuer,
                ValidAudience = audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!))
            };
        }
    }
}
