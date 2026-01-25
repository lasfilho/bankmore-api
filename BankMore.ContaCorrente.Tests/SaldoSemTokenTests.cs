using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using Xunit;

namespace BankMore.ContaCorrente.Tests;

public class SaldoSemTokenTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SaldoSemTokenTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Saldo_SemToken_DeveRetornar403()
    {
        var resp = await _client.GetAsync("/api/contas/saldo");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
