using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace BankMore.ContaCorrente.Tests;

public class CriarContaCpfInvalidoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CriarContaCpfInvalidoTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CriarConta_CpfInvalido_DeveRetornar400_ComTypeInvalidDocument()
    {
        // ajuste os nomes dos campos conforme seu CriarContaCommand (parece ser nomeTitular/cpf/senha)
        var payload = new
        {
            nomeTitular = "Teste",
            cpf = "123",           // inválido
            senha = "Senha@123"
        };

        var resp = await _client.PostAsJsonAsync("/api/contas", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await resp.Content.ReadAsStringAsync();
        json.Should().Contain("INVALID_DOCUMENT");
    }
}
