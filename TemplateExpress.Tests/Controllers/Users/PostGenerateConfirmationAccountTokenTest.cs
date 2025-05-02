using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TemplateExpress.Api.Controllers;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Validations.Users;

namespace TemplateExpress.Tests.Controllers.Users;

public class PostGenerateConfirmationAccountTokenTest
{

    [Fact(DisplayName = "Given the confirmation account token generation, when the token is generated, then return success")]
    public async Task Success()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        
        var userController = new UserController(userServiceMock.Object);

        var emailAndPasswordDto = new EmailAndPasswordDto("test@test.com", "testpassword");
        var validator = new LoginUserValidator();

        var jwtConfirmationAccountTokenDto = new JwtTokenDto("token");

        userServiceMock.Setup(u => u.GenerateConfirmationAccountTokenAsync(It.IsAny<EmailAndPasswordDto>(), validator))
            .ReturnsAsync(Result<JwtTokenDto>.Success(jwtConfirmationAccountTokenDto));
        
        // Act
        var result = await userController.PostGenerateConfirmationAccountToken(emailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        // Validate interactions
        userServiceMock.Verify(u => u.GenerateConfirmationAccountTokenAsync(It.IsAny<EmailAndPasswordDto>(),It.IsAny<IValidator<EmailAndPasswordDto>>()), Times.Once);
        
    }
    [Fact(DisplayName = "Given the confirmation account token generation, when the token don't is generated, then return failure")]
    public async Task Error()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        
        var userController = new UserController(userServiceMock.Object);

        var emailAndPasswordDto = new EmailAndPasswordDto("test@test.com", "testpassword");
        var validator = new LoginUserValidator();

        var jwtConfirmationAccountTokenDto = new JwtTokenDto("token");

        List<IErrorMessage> errorMessages = [new ErrorMessage("Invalid Email or Password.", "Post valid credentials.")];
        var error = new Error((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);

        
        userServiceMock.Setup(u => u.GenerateConfirmationAccountTokenAsync(It.IsAny<EmailAndPasswordDto>(), validator))
            .ReturnsAsync(Result<JwtTokenDto>.Failure(error));
        
        // Act
        var result = await userController.PostGenerateConfirmationAccountToken(emailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        
        // Validate interactions
        userServiceMock.Verify(u => u.GenerateConfirmationAccountTokenAsync(It.IsAny<EmailAndPasswordDto>(),It.IsAny<IValidator<EmailAndPasswordDto>>()), Times.Once);
        
    }
}