using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;

namespace TemplateExpress.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType<UserEmailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Error>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostUser([FromBody] CreateUserDto createUserDto)
    {
   
        var response = await _userService.CreateUserAsync(createUserDto);

        if (response.IsSuccess)
        {
            return Ok(response.Value);
        }
        if (response.Error?.Code == (byte)ErrorCodes.EmailAlreadyExists)
        {
            return Conflict(response.Error);    
        }
        
        return BadRequest(response.Error);
        
    }
}
