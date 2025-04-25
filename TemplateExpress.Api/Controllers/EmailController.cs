using Microsoft.AspNetCore.Mvc;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-confirmation-token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<Error>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostEmailConfirmationToken([FromBody] JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto)
    {
        var response = await _emailService.SendEmailConfirmationTokenAsync(jwtConfirmationAccountTokenDto);

        if (response.IsSuccess)
            return NoContent();

        return Unauthorized(response.Error);

    }
    
}