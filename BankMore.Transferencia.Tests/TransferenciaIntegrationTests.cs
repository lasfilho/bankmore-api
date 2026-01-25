using BankMore.Transferencia.Tests;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace BankMore.Transferencia.Tests;

public class TransferenciaIntegrationTests
{
    [Fact]
    public async Task Deve_transferir_50_de_origem_para_destino()
    {
        using var contaFactory = new ContaCorrenteFactory();
        using var conta = contaFactory.CreateClient(); // in-memory

        var contaHandler = contaFactory.Server.CreateHandler(); // ✅ handler do TestServer

        using var transfFactory = new CustomWebApplicationFactoryTransferencia(conta.BaseAddress!, contaHandler);
        using var transf = transfFactory.CreateClient(); // in-memory


        // 1) Criar contas
        var cpfA = GerarCpfValido();
        var cpfB = GerarCpfValido();
        const string senha = "Senha@123";

        var contaA = await CriarConta(conta, "Origem", cpfA, senha);
        var contaB = await CriarConta(conta, "Destino", cpfB, senha);

        // 2) Login origem + crédito 200
        var tokenA = await Login(conta, cpfA, senha);
        conta.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        await Movimentar(conta, requestId: Guid.NewGuid(), tipo: "C", valor: 200m, numeroConta: null);

        // 3) Transferir 50 para B
        transf.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var requestId = Guid.NewGuid();
        var respTransf = await transf.PostAsJsonAsync("/api/transferencias", new
        {
            requestId,
            numeroContaDestino = contaB,
            valor = 50m
        });

        respTransf.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // 4) Ver saldo destino = 50
        var tokenB = await Login(conta, cpfB, senha);

        using var contaBClient = contaFactory.CreateClient(); // ✅ mesma API in-memory
        contaBClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var saldoB = await contaBClient.GetFromJsonAsync<SaldoResp>("/api/contas/saldo");
        saldoB!.saldo.Should().Be(50m);

        // 5) Ver saldo origem = 150
        var saldoA = await conta.GetFromJsonAsync<SaldoResp>("/api/contas/saldo");
        saldoA!.saldo.Should().Be(150m);
    }

    [Fact]
    public async Task Repetir_mesma_transferencia_deve_ser_idempotente()
    {
        using var contaFactory = new ContaCorrenteFactory();
        using var conta = contaFactory.CreateClient(); // in-memory

        var contaHandler = contaFactory.Server.CreateHandler(); // ✅ handler do TestServer

        using var transfFactory = new CustomWebApplicationFactoryTransferencia(conta.BaseAddress!, contaHandler);
        using var transf = transfFactory.CreateClient(); // in-memory


        var cpfA = GerarCpfValido();
        var cpfB = GerarCpfValido();
        const string senha = "Senha@123";

        var contaA = await CriarConta(conta, "Origem", cpfA, senha);
        var contaB = await CriarConta(conta, "Destino", cpfB, senha);

        var tokenA = await Login(conta, cpfA, senha);
        conta.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        await Movimentar(conta, Guid.NewGuid(), "C", 200m, null);

        transf.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var requestId = Guid.NewGuid();

        var r1 = await transf.PostAsJsonAsync("/api/transferencias", new
        {
            requestId,
            numeroContaDestino = contaB,
            valor = 50m
        });
        r1.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // repete mesma requisição (mesmo requestId)
        var r2 = await transf.PostAsJsonAsync("/api/transferencias", new
        {
            requestId,
            numeroContaDestino = contaB,
            valor = 50m
        });
        r2.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // saldo destino não pode virar 100
        var tokenB = await Login(conta, cpfB, senha);

        using var contaBClient = contaFactory.CreateClient();
        contaBClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var saldoB = await contaBClient.GetFromJsonAsync<SaldoResp>("/api/contas/saldo");
        saldoB!.saldo.Should().Be(50m);
    }

    private static async Task<string> CriarConta(HttpClient conta, string nome, string cpf, string senha)
    {
        var resp = await conta.PostAsJsonAsync("/api/contas", new
        {
            nomeTitular = nome,
            cpf,
            senha
        });

        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<CreatedContaResp>();
        return json!.NumeroConta;
    }

    private static async Task<string> Login(HttpClient conta, string cpf, string senha)
    {
        var resp = await conta.PostAsJsonAsync("/api/contas/login", new { cpf, senha });
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<LoginResp>();
        return json!.Token;
    }

    private static async Task Movimentar(HttpClient conta, Guid requestId, string tipo, decimal valor, string? numeroConta)
    {
        var resp = await conta.PostAsJsonAsync("/api/contas/movimentos", new
        {
            requestId,
            numeroConta,
            tipo,
            valor
        });

        resp.EnsureSuccessStatusCode();
    }

    private static string GerarCpfValido()
    {
        int[] n = new int[9];
        for (int i = 0; i < 9; i++)
            n[i] = Random.Shared.Next(0, 10);

        int d1 = CalcularDigito(n, 10);
        int d2 = CalcularDigito(n.Concat(new[] { d1 }).ToArray(), 11);

        return string.Concat(n) + d1 + d2;

        static int CalcularDigito(int[] digits, int pesoInicial)
        {
            int soma = 0;
            for (int i = 0; i < digits.Length; i++)
                soma += digits[i] * (pesoInicial - i);

            int r = soma % 11;
            return (r < 2) ? 0 : (11 - r);
        }
    }

    private sealed class CreatedContaResp
    {
        public string NumeroConta { get; set; } = "";
    }

    private sealed class LoginResp
    {
        public string Token { get; set; } = "";
    }

    private sealed class SaldoResp
    {
        public decimal saldo { get; set; }
    }
}
