using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TemplateExpress.Api.Controllers;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;

namespace TemplateExpress.Tests.Controllers;

public class UsersControllerTest
{

    [Fact(DisplayName = "Given an User, when it is valid, then should return OK.")]
    public async Task PostTest_ValidUser()
    {
        
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userValidationMock = new Mock<IValidator<CreateUserDto>>();
        var usersController = new UsersController(userServiceMock.Object);
        
        var createUserDto = new CreateUserDto("test@test.com", "test_user", "12L0d1xP-!@dX");
        var mockServiceResponse = Result<UserEmailDto>.Success(new UserEmailDto(createUserDto.Email));
        
        userServiceMock
            .Setup(s => s.CreateUserAsync(userValidationMock.Object, createUserDto))
            .ReturnsAsync(mockServiceResponse);

        // Act
        var result = await usersController.PostUser(userValidationMock.Object, createUserDto);
        var okObjectResult = result as OkObjectResult;
        var resultValue = okObjectResult?.Value as UserEmailDto;

        // Assert
        result.Should().BeOfType<OkObjectResult>("the user was created successfully.");
        resultValue?.Email.Should().NotBeNull().And.Be(createUserDto.Email);
        userServiceMock.Verify(s => s.CreateUserAsync(userValidationMock.Object, createUserDto), Times.Once());
        userValidationMock.Verify(v => v.ValidateAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact(DisplayName = "Given an User, when it is invalid, then should return BadRequest.")]
    public async Task PostTest_InvalidUser()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userValidationMock = new Mock<IValidator<CreateUserDto>>();
        var usersController = new UsersController(userServiceMock.Object);
        
        var createUserDto = new CreateUserDto("test@test.com", "test_user", "12L0d1xP-!@dX");
        var errorMessages = new List<IErrorMessage> 
        { 
            new ErrorMessage("Invalid input", "Check the fields.") 
        };
        
        var mockServiceResponse = Result<UserEmailDto>
            .Failure(new Error((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.InputValidationError, errorMessages));
        
        userServiceMock
            .Setup(s => s.CreateUserAsync(userValidationMock.Object, createUserDto))
            .ReturnsAsync(mockServiceResponse);
    
        // Act
        var result = await usersController.PostUser(userValidationMock.Object, createUserDto);
        
        var badRequestObjectResult = result as BadRequestObjectResult;
        var resultValue = badRequestObjectResult?.Value as Error;
    
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("the user wasn't created successfully.");
        resultValue?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        resultValue?.Type.Should().Be((byte)ErrorTypes.InputValidationError);
        userServiceMock.Verify(s => s.CreateUserAsync(userValidationMock.Object, createUserDto), Times.Once());
        userValidationMock.Verify(v => v.ValidateAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()), Times.Never());
    
    }
    
    [Fact(DisplayName = "Given a User, when it is conflicting, then should return Conflict.")]
    public async Task PostTest_ConflictingUser()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userValidationMock = new Mock<IValidator<CreateUserDto>>();
        var usersController = new UsersController(userServiceMock.Object);
        
        var createUserDto = new CreateUserDto("test@test.com", "test_user", "12L0d1xP-!@dX");
        var errorMessages = new List<IErrorMessage> 
        { 
            new ErrorMessage("Invalid input", "Check the fields.") 
        };
        
        var mockServiceResponse = Result<UserEmailDto>.Failure(new Error((byte)ErrorCodes.UsernameAlreadyExists, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages));
        userServiceMock.Setup(s => s.CreateUserAsync(userValidationMock.Object, createUserDto)).ReturnsAsync(mockServiceResponse);
    
        // Act
        var result = await usersController.PostUser(userValidationMock.Object, createUserDto);
    
        var conflictObjectResult = result as ConflictObjectResult;
        var resultValue = conflictObjectResult?.Value as Error;
    
        // Assert
        result.Should().BeOfType<ConflictObjectResult>("the user username already exists.");
        resultValue?.Code.Should().Be((byte)ErrorCodes.UsernameAlreadyExists);
        resultValue?.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        userServiceMock.Verify(s => s.CreateUserAsync(userValidationMock.Object, createUserDto), Times.Once());
        userValidationMock.Verify(v => v.ValidateAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()), Times.Never());
    
    }
    
}