using FluentAssertions;
using Moq;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Services;
using TemplateExpress.Api.Validations.Users;

namespace TemplateExpress.Tests.Services.Users;

public class LoginAsyncTest
{

    [Fact(DisplayName = "Given the user login, when it exists and is validated, then it should return a access token")]
    public async Task Success()
    {
        // Arrange
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        var mocks = DefaultMocks.GetAllMocks();
        var validator = new LoginUserValidator();
        
        mocks.userRepositoryMock.Setup(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()))
            .ReturnsAsync(defaultObjects.userEntity);

        mocks.bcryptUtilMock.Setup(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        
        mocks.tokenManagerMock.Setup(t => t.GenerateAuthenticationToken(It.IsAny<UserIdAndRoleDto>()))
            .Returns("token");
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        
        // Act
        var result = await userService.LoginAsync(defaultObjects.emailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        
        // Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GenerateAuthenticationToken(It.IsAny<UserIdAndRoleDto>()), Times.Once);
    }
    
    [Fact(DisplayName = "Given the user login, when it is invalid, then it should return a validation error")]
    public async Task InvalidUser()
    {
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();
        var validator = new LoginUserValidator();
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        
        var invalidEmailAndPasswordDto = new EmailAndPasswordDto("", "123123");
        
        // Act
        var result = await userService.LoginAsync(invalidEmailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<Error>();
        result.Error!.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error!.Type.Should().Be((byte)ErrorTypes.InputValidationError);
        result.Error!.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        
        // Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Never);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Never);
    }
    
    [Fact(DisplayName = "Given the user login, when it the user don't exists, then it should return a business validation error")]
    public async Task WrongUserEmail()
    {
        // Arrange
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        var mocks = DefaultMocks.GetAllMocks();
        var validator = new LoginUserValidator();

        mocks.userRepositoryMock.Setup(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()));
            
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        
        // Act
        var result = await userService.LoginAsync(defaultObjects.emailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<Error>();
        result.Error!.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error!.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        result.Error!.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        // Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        mocks.tokenManagerMock.Verify(t => t.GenerateAuthenticationToken(It.IsAny<UserIdAndRoleDto>()), Times.Never);
    }
    
    
    [Fact(DisplayName = "Given the user login, when it the user password is wrong, then it should return a business validation error")]
    public async Task WrongUserPassword()
    {
        // Arrange
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        var mocks = DefaultMocks.GetAllMocks();
        var validator = new LoginUserValidator();
        
        mocks.userRepositoryMock.Setup(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()))
            .ReturnsAsync(defaultObjects.userEntity);

        mocks.bcryptUtilMock.Setup(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        
        // Act
        var result = await userService.LoginAsync(defaultObjects.emailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<Error>();
        result.Error!.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error!.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        result.Error!.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        // Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GenerateAuthenticationToken(It.IsAny<UserIdAndRoleDto>()), Times.Never);
    }
    
}

