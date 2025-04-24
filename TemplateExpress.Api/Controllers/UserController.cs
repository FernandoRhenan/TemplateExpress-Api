using Microsoft.AspNetCore.Mvc;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;

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
    [ProducesResponseType<UserEmailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostUser([FromBody] CreateUserDto createUserDto)
    {
   
        var response = await _userService.CreateUserAndTokenAsync(createUserDto);

        if (response.IsSuccess) 
            return Ok(response.Value);
        
        if (response.Error?.Code == (byte)ErrorCodes.EmailAlreadyExists) 
            return Conflict(response.Error);    
        
        return BadRequest(response.Error);
        
    } 
    
    [HttpPatch("email-confirmation/{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<Error>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PatchConfirmAccount(string token)
    {

        var response = await _userService.ConfirmAccountAsync(new JwtConfirmationAccountTokenDto(token));
        
        if(response.IsSuccess)
            return NoContent();
        
        return Unauthorized(response.Error);


    }
}
