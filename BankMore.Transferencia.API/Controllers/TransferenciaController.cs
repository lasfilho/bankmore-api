using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Transferencia.API.Controllers;

[ApiController]
[Route("api/transferencias")]
public sealed class TransferenciaController : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Efetuar(
        [FromBody] EfetuarTransferenciaCommand command,
        [FromServices] EfetuarTransferenciaCommandHandler handler,
        CancellationToken ct)
    {
        try
        {
            await handler.Handle(command, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            // regra: 403 quando token inválido/expirado
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                TipoFalha = "USER_UNAUTHORIZED",
                Mensagem = ex.Message
            });
        }
        catch (BusinessException ex)
        {
            // regra: 400 quando dados inconsistentes / falha em alguma requisição
            return BadRequest(new
            {
                TipoFalha = ex.TipoFalha,
                Mensagem = ex.Message
            });
        }
    }
}
