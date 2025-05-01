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

    private (CreateUserDto createUserDto, List<IErrorMessage> errorMessages) GenerateDefaultObjects()
    {
        var createUserDto = new CreateUserDto("test@test.com", "comusertest1", "=d#OdcA)53?p7$$$Sv_0 ");
        
        var errorMessages = new List<IErrorMessage>
        {
            new ErrorMessage("Invalid input", "Check the fields.")
        };
        
        return (createUserDto, errorMessages);
    }
    
    
    [Fact(DisplayName = "Given a PostUser request, when it is all right, then return a success response.")]
    public async Task ValidUser()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var userController = new UserController(userServiceMock.Object);
        var jwtConfirmationAccountToken = new JwtConfirmationAccountTokenDto("token");
        var validator = new CreateUserValidator();
        var defaultObjects = GenerateDefaultObjects();
        
        var mockServiceResponse = Result<JwtConfirmationAccountTokenDto>.Success(jwtConfirmationAccountToken);
        
        userServiceMock
            .Setup(s => s.CreateUserAndTokenAsync(defaultObjects.createUserDto, validator))
            .ReturnsAsync(mockServiceResponse);

        // Act
        var result = await userController.PostUser(defaultObjects.createUserDto, validator);
        
        // Assert
        var okObjectResult = result as OkObjectResult;
        var resultValue = okObjectResult?.Value as JwtConfirmationAccountTokenDto;

        result.Should().BeOfType<OkObjectResult>();
        resultValue?.Token.Should().NotBeNull().And.Be(jwtConfirmationAccountToken.Token);
        userServiceMock.Verify(s => s.CreateUserAndTokenAsync(It.IsAny<CreateUserDto>(), It.IsAny<IValidator<CreateUserDto>>()), Times.Once());
    }

    [Fact(DisplayName = "Given a PostUser request, when there is a validation error, then return a failed response.")]

    public async Task InvalidUser()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var usersController = new UserController(userServiceMock.Object);
        var validator = new CreateUserValidator();
        var defaultObjects = GenerateDefaultObjects();

        
        var mockServiceResponse = Result<JwtConfirmationAccountTokenDto>
            .Failure(new Error((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.InputValidationError, defaultObjects.errorMessages));
        
        userServiceMock
            .Setup(s => s.CreateUserAndTokenAsync(defaultObjects.createUserDto, validator))
            .ReturnsAsync(mockServiceResponse);
    
        // Act
        var result = await usersController.PostUser(defaultObjects.createUserDto, validator);
        
        var badRequestObjectResult = result as BadRequestObjectResult;
        var resultValue = badRequestObjectResult?.Value as Error;
    
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("the user wasn't created successfully.");
        resultValue?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        resultValue?.Type.Should().Be((byte)ErrorTypes.InputValidationError);
        userServiceMock.Verify(s => s.CreateUserAndTokenAsync(It.IsAny<CreateUserDto>(), It.IsAny<IValidator<CreateUserDto>>()), Times.Once());
    
    }
    
    [Fact(DisplayName = "Given a PostUser request, when there is a business validation error, then return a failed response.")]
    public async Task ConflictingUser()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var usersController = new UserController(userServiceMock.Object);
        var validator = new CreateUserValidator();
        var defaultObjects = GenerateDefaultObjects();

        
        var mockServiceResponse = Result<JwtConfirmationAccountTokenDto>.Failure(new Error((byte)ErrorCodes.EmailAlreadyExists, (byte)ErrorTypes.BusinessLogicValidationError, defaultObjects.errorMessages));
        userServiceMock.Setup(s => s.CreateUserAndTokenAsync(defaultObjects.createUserDto, validator)).ReturnsAsync(mockServiceResponse);
    
        // Act
        var result = await usersController.PostUser(defaultObjects.createUserDto, validator);
    
        var conflictObjectResult = result as ConflictObjectResult;
        var resultValue = conflictObjectResult?.Value as Error;
    
        // Assert
        result.Should().BeOfType<ConflictObjectResult>("the user username already exists.");
        resultValue?.Code.Should().Be((byte)ErrorCodes.EmailAlreadyExists);
        resultValue?.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        userServiceMock.Verify(s => s.CreateUserAndTokenAsync(It.IsAny<CreateUserDto>(), It.IsAny<IValidator<CreateUserDto>>()), Times.Once());
    
    }
    
}