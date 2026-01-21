using BankMore.Transferencia.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BankMore.Transferencia.Application.Clients;

public sealed class ContaCorrenteClient
{
    private readonly HttpClient _http;

    public ContaCorrenteClient(HttpClient http) => _http = http;

    public async Task<HttpResponseMessage> MovimentarAsync(MovimentarContaRequest req, string bearerToken, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(req);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        // Seu endpoint atual: POST /api/contas/movimentos
        return await _http.PostAsync("/api/contas/movimentos", content, ct);
    }
}
