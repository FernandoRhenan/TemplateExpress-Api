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

public class PostLoginTest
{
    
    [Fact(DisplayName = "Given the user login, when the user is authentic, then return success")]
    public async Task Success()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        
        var jwtTokenDto = new JwtTokenDto("token");

        userServiceMock.Setup(u => u.LoginAsync(It.IsAny<EmailAndPasswordDto>(), It.IsAny<IValidator<EmailAndPasswordDto>>()))
            .ReturnsAsync(Result<JwtTokenDto>.Success(jwtTokenDto));

        var userEmailAndPasswordDto = new EmailAndPasswordDto("test@test.com", "123123");
        var validator = new LoginUserValidator();
        
        // Act
        var result = await userController.PostLogin(userEmailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        // Validate interactions
        userServiceMock.Verify(u => u.LoginAsync(It.IsAny<EmailAndPasswordDto>(), It.IsAny<IValidator<EmailAndPasswordDto>>()), Times.Once);
    }    
    
    [Fact(DisplayName = "Given the user login, when the user don't is authentic, then return error")]
    public async Task Error()
    {
        // Arrange
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        
        var expectedResult = Result<JwtTokenDto>
            .Failure(new Error((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.InputValidationError, defaultObjects.errorMessages));
        
        userServiceMock.Setup(u => u.LoginAsync(It.IsAny<EmailAndPasswordDto>(), It.IsAny<IValidator<EmailAndPasswordDto>>()))
            .ReturnsAsync(expectedResult);

        var userEmailAndPasswordDto = new EmailAndPasswordDto("test@test.com", "123123");
        var validator = new LoginUserValidator();
        
        // Act
        var result = await userController.PostLogin(userEmailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        
        // Validate interactions
        userServiceMock.Verify(u => u.LoginAsync(It.IsAny<EmailAndPasswordDto>(), It.IsAny<IValidator<EmailAndPasswordDto>>()), Times.Once);
    }
    
}