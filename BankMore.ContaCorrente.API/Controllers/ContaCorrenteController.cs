using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.ContaCorrente.API.Controllers
{
    [ApiController]
    [Route("api/contas")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly CriarContaCommandHandler _criarContaHandler;
        private readonly LoginContaCommandHandler _loginContaHandler;

        public ContaCorrenteController(CriarContaCommandHandler criarContaHandler, LoginContaCommandHandler loginContaHandler)
        {
            _criarContaHandler = criarContaHandler;
            _loginContaHandler = loginContaHandler;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CriarConta([FromBody] CriarContaCommand command)
        {
            var numeroConta = await _criarContaHandler.Handle(command);
            return Created("", new { NumeroConta = numeroConta });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginContaCommand command)
        {
            var token = await _loginContaHandler.Handle(command);
            return Ok(new { Token = token });
        }

        [HttpPost("inativar")]
        [Authorize]
        public async Task<IActionResult> InativarConta(
            [FromBody] InativarContaCommand command,
            [FromServices] InativarContaCommandHandler handler)
        {
            await handler.Handle(command);
            return NoContent();
        }

        [Authorize]
        [HttpPost("movimentos")]
        public async Task<IActionResult> Movimentar(
            [FromBody] MovimentarContaCommand command,
            [FromServices] MovimentarContaCommandHandler handler)
        {
            await handler.Handle(command);
            return NoContent();
        }

        [Authorize]
        [HttpGet("saldo")]
        public async Task<IActionResult> Saldo([FromServices] SaldoContaQueryHandler handler)
        {
            var resp = await handler.Handle();
            return Ok(resp);
        }
    }
}
