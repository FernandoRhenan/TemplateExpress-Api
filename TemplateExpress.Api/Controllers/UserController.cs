using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType<JwtTokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostUser([FromBody] CreateUserDto createUserDto, [FromServices] IValidator<CreateUserDto> validator)
    {
        var response = await _userService.CreateUserAndTokenAsync(createUserDto, validator);

        if (response.IsSuccess) 
            return Ok(response.Value);
        
        if (response.Error?.Code == (byte)ErrorCodes.EmailAlreadyExists) 
            return Conflict(response.Error);    
        
        return BadRequest(response.Error);
    } 
    
    [HttpPatch("email-confirmation/{token}")]
    [ProducesResponseType<JwtTokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PatchConfirmAccount(string token)
    {
        var response = await _userService.ConfirmAccountAsync(new JwtTokenDto(token));
        
        if(response.IsSuccess)
            return Ok(response.Value);
        
        return Unauthorized(response.Error);
    }
    
    [HttpPost("generate-confirmation-account-token")]
    [ProducesResponseType<JwtTokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostGenerateConfirmationAccountToken([FromBody] EmailAndPasswordDto emailAndPasswordDto, [FromServices] IValidator<EmailAndPasswordDto> validator)
    {

        var response = await _userService.GenerateConfirmationAccountTokenAsync(emailAndPasswordDto, validator);
        
        if(response.IsSuccess)
            return Ok(response.Value);
        
        return BadRequest(response.Error);
            
    }


    [HttpPost("login")]
    [ProducesResponseType<JwtTokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostLogin([FromBody] EmailAndPasswordDto emailAndPasswordDto, [FromServices] IValidator<EmailAndPasswordDto> validator)
    {
        var response = await _userService.LoginAsync(emailAndPasswordDto, validator);
        
        if(response.IsSuccess)
            return Ok(response.Value);
        
        return BadRequest(response.Error);
    }
    
}
