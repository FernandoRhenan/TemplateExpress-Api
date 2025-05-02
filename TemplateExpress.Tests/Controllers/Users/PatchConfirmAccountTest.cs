using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TemplateExpress.Api.Controllers;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Tests.Controllers.Users;

public class PatchConfirmAccountTest
{

    [Fact(DisplayName = "Given the user confirm account updating, when the user is confirmed, then return a success response without response body")]
    public async Task Success()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        const string token = "token";
        var jwtConfirmationAccountTokenDto = new JwtTokenDto(token);

        userServiceMock.Setup(u => u.ConfirmAccountAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(Result<string>.Success(String.Empty));
        
        // Act
        var result = await userController.PatchConfirmAccount(token);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify interactions
        userServiceMock.Verify(u => u.ConfirmAccountAsync(jwtConfirmationAccountTokenDto), Times.Once);

    }
    
    [Fact(DisplayName = "Given the user confirm account updating, when the user don't is confirmed, then return a error response.")]
    public async Task Error()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        const string token = "token";
        var jwtConfirmationAccountTokenDto = new JwtTokenDto(token);

        List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
        var error = Result<string>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        userServiceMock.Setup(u => u.ConfirmAccountAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(error);
        
        // Act
        var result = await userController.PatchConfirmAccount(token);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        
        // Verify interactions
        userServiceMock.Verify(u => u.ConfirmAccountAsync(jwtConfirmationAccountTokenDto), Times.Once);

    }
    
}