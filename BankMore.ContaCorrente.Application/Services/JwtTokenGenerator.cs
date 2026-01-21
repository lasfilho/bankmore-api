using ContaCorrenteEntity = BankMore.ContaCorrente.Domain.Entities.ContaCorrente;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.Application.Services
{
    public static class JwtTokenGenerator
    {
        public static string GerarToken(ContaCorrenteEntity conta, IConfiguration configuration)
        {
            var secret = configuration["Jwt:Secret"];
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtClaims.ContaId, conta.Id.ToString()),
                new Claim(JwtClaims.NumeroConta, conta.NumeroConta),
                new Claim(JwtClaims.Cpf, conta.Cpf)
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
