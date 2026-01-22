using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BankMore.ContaCorrente.API.Controllers
{
    [ApiController]
    [Route("api/contas")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly CriarContaCommandHandler _criarContaHandler;
        private readonly LoginContaCommandHandler _loginContaHandler;

        public ContaCorrenteController(
            CriarContaCommandHandler criarContaHandler,
            LoginContaCommandHandler loginContaHandler)
        {
            _criarContaHandler = criarContaHandler;
            _loginContaHandler = loginContaHandler;
        }

        [HttpPost]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Cadastra uma conta corrente",
            Description = "Cria uma conta corrente a partir do CPF e senha. Retorna o número da conta."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Conta criada com sucesso")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "CPF inválido ou dados inconsistentes (type=INVALID_DOCUMENT)")]
        public async Task<IActionResult> CriarConta([FromBody] CriarContaCommand command)
        {
            var numeroConta = await _criarContaHandler.Handle(command);
            return Created("", new { NumeroConta = numeroConta });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Efetua login",
            Description = "Autentica o usuário e retorna um token JWT com a identificação da conta corrente."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Login realizado com sucesso (retorna token JWT)")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "CPF/conta ou senha inválidos (type=USER_UNAUTHORIZED)")]
        public async Task<IActionResult> Login([FromBody] LoginContaCommand command)
        {
            var token = await _loginContaHandler.Handle(command);
            return Ok(new { Token = token });
        }

        [HttpPost("inativar")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Inativa a conta corrente",
            Description = "Inativa a conta da sessão (token). Exige senha da conta."
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Conta inativada com sucesso")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inconsistentes ou conta inválida/inativa")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Token inválido ou expirado")]
        public async Task<IActionResult> InativarConta(
            [FromBody] InativarContaCommand command,
            [FromServices] InativarContaCommandHandler handler)
        {
            await handler.Handle(command);
            return NoContent();
        }

        [HttpPost("movimentos")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Realiza movimentação (crédito/débito)",
            Description = "Movimenta a conta do token. Se NumeroConta for informado e diferente da conta logada, somente crédito é permitido."
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Movimentação realizada com sucesso")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inconsistentes (INVALID_ACCOUNT/INACTIVE_ACCOUNT/INVALID_VALUE/INVALID_TYPE)")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Token inválido ou expirado")]
        public async Task<IActionResult> Movimentar(
            [FromBody] MovimentarContaCommand command,
            [FromServices] MovimentarContaCommandHandler handler)
        {
            await handler.Handle(command);
            return NoContent();
        }

        [HttpGet("saldo")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Consulta saldo",
            Description = "Retorna o saldo atual da conta corrente do token (créditos - débitos)."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Saldo retornado com sucesso")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Conta inválida ou inativa")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Token inválido ou expirado")]
        public async Task<IActionResult> Saldo([FromServices] SaldoContaQueryHandler handler)
        {
            var resp = await handler.Handle();
            return Ok(resp);
        }
    }
}

