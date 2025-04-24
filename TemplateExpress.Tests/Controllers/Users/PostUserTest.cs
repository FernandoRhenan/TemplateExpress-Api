using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TemplateExpress.Api.Controllers;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;
using TemplateExpress.Api.Validations.Users;

namespace TemplateExpress.Tests.Controllers.Users;

public class PostUserTest
{
    // TODO: Review this test and check de logic
    [Fact(DisplayName = "Given an User, when it is valid, then should return OK.")]
    public async Task ValidUser()
    {
        
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        var jwtConfirmationAccountToken = new JwtConfirmationAccountTokenDto("token");
        
        var createUserDto = new CreateUserDto("test@test.com", "test_user", "12L0d1xP-!@dX");
        var validator = new CreateUserValidator();
        var mockServiceResponse = Result<JwtConfirmationAccountTokenDto>.Success(jwtConfirmationAccountToken);
        
        userServiceMock
            .Setup(s => s.CreateUserAndTokenAsync(createUserDto, validator))
            .ReturnsAsync(mockServiceResponse);

        // Act
        var result = await userController.PostUser(createUserDto, validator);
        
        // Assert
        var okObjectResult = result as OkObjectResult;
        var resultValue = okObjectResult?.Value as JwtConfirmationAccountTokenDto;

        result.Should().BeOfType<OkObjectResult>();
        resultValue?.Token.Should().NotBeNull().And.Be(jwtConfirmationAccountToken.Token);
        userServiceMock.Verify(s => s.CreateUserAndTokenAsync(createUserDto, validator), Times.Once());
    }

    // TODO: Review this test and check de logic
    [Fact(DisplayName = "Given an User, when it is invalid, then should return BadRequest.")]
    public async Task InvalidUser()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var usersController = new UserController(userServiceMock.Object);
        
        var createUserDto = new CreateUserDto("test@test.com", "test_user", "12L0d1xP-!@dX");
        var errorMessages = new List<IErrorMessage> 
        { 
            new ErrorMessage("Invalid input", "Check the fields.") 
        };
        
        var mockServiceResponse = Result<JwtConfirmationAccountTokenDto>
            .Failure(new Error((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.InputValidationError, errorMessages));
        
        var validator = new CreateUserValidator();
        
        userServiceMock
            .Setup(s => s.CreateUserAndTokenAsync(createUserDto, validator))
            .ReturnsAsync(mockServiceResponse);
    
        // Act
        var result = await usersController.PostUser(createUserDto, validator);
        
        var badRequestObjectResult = result as BadRequestObjectResult;
        var resultValue = badRequestObjectResult?.Value as Error;
    
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("the user wasn't created successfully.");
        resultValue?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        resultValue?.Type.Should().Be((byte)ErrorTypes.InputValidationError);
        userServiceMock.Verify(s => s.CreateUserAndTokenAsync(createUserDto, validator), Times.Once());
    
    }
    
    // TODO: Review this test and check de logic
    [Fact(DisplayName = "Given an User, when it is conflicting, then should return Conflict.")]
    public async Task ConflictingUser()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var usersController = new UserController(userServiceMock.Object);
        
        var createUserDto = new CreateUserDto("test@test.com", "test_user", "12L0d1xP-!@dX");
        var errorMessages = new List<IErrorMessage> 
        { 
            new ErrorMessage("Invalid input", "Check the fields.") 
        };
        
        var validator = new CreateUserValidator();

        
        var mockServiceResponse = Result<JwtConfirmationAccountTokenDto>.Failure(new Error((byte)ErrorCodes.EmailAlreadyExists, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages));
        userServiceMock.Setup(s => s.CreateUserAndTokenAsync(createUserDto, validator)).ReturnsAsync(mockServiceResponse);
    
        // Act
        var result = await usersController.PostUser(createUserDto, validator);
    
        var conflictObjectResult = result as ConflictObjectResult;
        var resultValue = conflictObjectResult?.Value as Error;
    
        // Assert
        result.Should().BeOfType<ConflictObjectResult>("the user username already exists.");
        resultValue?.Code.Should().Be((byte)ErrorCodes.EmailAlreadyExists);
        resultValue?.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        userServiceMock.Verify(s => s.CreateUserAndTokenAsync(createUserDto, validator), Times.Once());
    
    }
    
}