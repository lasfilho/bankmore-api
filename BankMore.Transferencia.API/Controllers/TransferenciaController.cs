using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BankMore.Transferencia.API.Controllers;

[ApiController]
[Route("api/transferencias")]
public sealed class TransferenciaController : ControllerBase
{
    [Authorize]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Efetua transferência entre contas da mesma instituição",
        Description = "Debita a conta logada e credita a conta destino. Em falha no crédito, realiza estorno."
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Transferência realizada com sucesso")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inconsistentes ou falha em alguma requisição (type variado)")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Token inválido ou expirado (type=USER_UNAUTHORIZED)")]
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
                message = ex.Message,
                type = "USER_UNAUTHORIZED"
            });
        }
        catch (BusinessException ex)
        {
            // regra: 400 quando dados inconsistentes / falha em alguma requisição
            return BadRequest(new
            {
                message = ex.Message,
                type = ex.TipoFalha
            });
        }
    }
}
