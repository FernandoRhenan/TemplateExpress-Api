using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;
using TemplateExpress.Api.Services;
using TemplateExpress.Api.Validations.Users;

namespace TemplateExpress.Tests.Services.Users;

public class GenerateConfirmationAccountTokenAsyncTest
{
    private static readonly DateTime Now = DateTime.Now;
    private static readonly Random Random = new();
    
    private (UserEntity userEntity, CreateUserDto createUserDto, EmailAndPasswordDto emailAndPasswordDto) GenerateDefaultObjects()
    {
        var userId = (long)Random.Next(1, 50_000);

        var createUserDto = new CreateUserDto("test@test.com", "usertest1", "=d#OdcA)53?p7$$$Sv_0 ");
        
        var userEntity = new UserEntity
        {
            Id = userId,
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            Username = createUserDto.Username,
            CreatedAt = Now,
            UpdatedAt = Now
        };
        
        var emailAndPasswordDto = new EmailAndPasswordDto(createUserDto.Email, createUserDto.Password);

        return (userEntity, createUserDto, emailAndPasswordDto);
    }

    private (Mock<IUserRepository> userRepositoryMock, Mock<IBCryptUtil> bcryptUtilMock, Mock<ITokenManager> tokenManagerMock) GetAllMocks()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var bcryptUtilMock = new Mock<IBCryptUtil>();
        var tokenManagerMock = new Mock<ITokenManager>();
        
        return (userRepositoryMock, bcryptUtilMock, tokenManagerMock);
    }

    [Fact(DisplayName = "Given the Confirmation Account Token Generation, when the user data is valid, then return a success response with the confirmation token.")]
    public async Task Success()
    {
        // Arrange
        var defaultObjects = GenerateDefaultObjects();
        var mocks = GetAllMocks();

        mocks.userRepositoryMock.Setup(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()))
            .ReturnsAsync(defaultObjects.userEntity);

        mocks.bcryptUtilMock.Setup(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        mocks.tokenManagerMock.Setup(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()))
            .Returns("token");
        
        var expectedResult = Result<JwtConfirmationAccountTokenDto>.Success(new JwtConfirmationAccountTokenDto("token")); 
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        var validator = new LoginUserValidator();

        // Act
        var result = await userService.GenerateConfirmationAccountTokenAsync(defaultObjects.emailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtConfirmationAccountTokenDto>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        
        // Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Once);;

    }

    [Fact(DisplayName = "Given the Confirmation Account Token Generation, when the user data is invalid, then return a validation error response.")]
    public async Task InvalidUser()
    {
        var mocks = GetAllMocks();

        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        
        var invalidEmailAndPasswordDto = new EmailAndPasswordDto("", "123123");
        var validator = new LoginUserValidator();

        // Act
        var result = await userService.GenerateConfirmationAccountTokenAsync(invalidEmailAndPasswordDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtConfirmationAccountTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error?.Type.Should().Be((byte)ErrorTypes.InputValidationError);
        result.Error?.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Never);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Never);
    }

    [Fact(DisplayName = "Given the Confirmation Account Token Generation, when the user does not exist, then return a validation error.")]
    public async Task UserNotFound()
    {
        // Arrange
        var defaultObjects = GenerateDefaultObjects();
        var mocks = GetAllMocks();
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("Invalid Email or Password.", "Post valid credentials.")];
        Error error = new((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);

        mocks.userRepositoryMock.Setup(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()));
        
        var expectedResult = Result<JwtConfirmationAccountTokenDto>.Failure(error); 
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        var validator = new LoginUserValidator();

        // Act
        var result = await userService.GenerateConfirmationAccountTokenAsync(defaultObjects.emailAndPasswordDto, validator);
        
        // Assert
        result.Should().BeOfType<Result<JwtConfirmationAccountTokenDto>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error?.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        result.Error?.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        // Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Never);
    }
    
    [Fact(DisplayName = "Given the Confirmation Account Token Generation, when the user password is wrong, then return a validation error.")]
    public async Task InvalidPassword()
    {
        // Arrange
        var defaultObjects = GenerateDefaultObjects();
        var mocks = GetAllMocks();
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("Invalid Email or Password.", "Post valid credentials.")];
        Error error = new((byte)ErrorCodes.InvalidInput, (byte)ErrorTypes.BusinessLogicValidationError, errorMessages);

        mocks.userRepositoryMock.Setup(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()))
            .ReturnsAsync(defaultObjects.userEntity);

        mocks.bcryptUtilMock.Setup(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        
        var expectedResult = Result<JwtConfirmationAccountTokenDto>.Failure(error); 
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        var validator = new LoginUserValidator();

        // Act
        var result = await userService.GenerateConfirmationAccountTokenAsync(defaultObjects.emailAndPasswordDto, validator);
        
        // Assert
        result.Should().BeOfType<Result<JwtConfirmationAccountTokenDto>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error?.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        result.Error?.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        // Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.ComparePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Never);
    }
    
}