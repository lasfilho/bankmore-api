using System.Net.Http.Json;
using BankMore.Transferencia.Application.DTOs;

namespace BankMore.Transferencia.Application.Clients;

public sealed class ContaCorrenteClient
{
    private readonly HttpClient _http;

    public ContaCorrenteClient(HttpClient http) => _http = http;

    public Task<HttpResponseMessage> MovimentarAsync(MovimentarContaRequest request, string token, CancellationToken ct)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/contas/movimentos")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return _http.SendAsync(httpRequest, ct);
    }
}
