using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TemplateExpress.Api.Dto.UserDtos;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;

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
    [ProducesResponseType<Error>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Error>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostUser([FromServices] IValidator<CreateUserDto> userValidator, [FromBody] CreateUserDto createUserDto)
    {
   
        var response = await _userService.CreateUserAsync(userValidator, createUserDto);

        if (response.IsSuccess)
        {
            return Ok(response.Value);
        }
        
        if (response.Error?.Code == "UsernameAlreadyExists")
        {
            return Conflict(response.Error);    
        }
        
        return BadRequest(response.Error);
        
    }
}
