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
using TemplateExpress.Api.Validations;

namespace TemplateExpress.Tests.Services;

public class UserServiceTests
{

    private static readonly DateTime Now = DateTime.Now;
    private static readonly Random Random = new();
    
    private (UserEntity userEntity, EmailConfirmationTokenEntity emailConfirmationTokenEntity, CreateUserDto createUserDto, UserIdAndEmailDto userIdAndEmailDto, UserEmailDto userEmailDto) GenerateDefaultObjects()
    {
        var userId = Random.Next(1, 50_000);

        var createUserDto = new CreateUserDto("test@test.com", "usertest1", "=d#OdcA)53?p7$$$Sv_0 ");
        var userIdAndEmailDto = new UserIdAndEmailDto(userId, createUserDto.Email);

        var userEmailDto = new UserEmailDto(createUserDto.Email);
        
        var userEntity = new UserEntity
        {
            Id = userId,
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            Username = createUserDto.Username,
            CreatedAt = Now,
            UpdatedAt = Now
        };

        var emailConfirmationTokenEntity = new EmailConfirmationTokenEntity
        {
            Id = userId,
            UserId = userId,
            Token = "someRandomToken",
            CreatedAt = Now,
            UpdatedAt = Now
        };

        return (userEntity, emailConfirmationTokenEntity, createUserDto, userIdAndEmailDto, userEmailDto);
    }

    private (Mock<IUserRepository> userRepositoryMock, Mock<IBCryptUtil> bcryptUtilMock, Mock<ITokenManager>
        tokenManagerMock, Mock<IDbContextTransaction> transactionMock) GetAllMocks()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var bcryptUtilMock = new Mock<IBCryptUtil>();
        var tokenManagerMock = new Mock<ITokenManager>();
        var transactionMock = new Mock<IDbContextTransaction>();
        return (userRepositoryMock, bcryptUtilMock, tokenManagerMock, transactionMock);
    }
    
    [Fact(DisplayName = "Given the user and token creation service, when the user data is valid, then the service should return a successResponse.")]
    public async Task CreateUserTest_succefully()
    {
        // Arrange
        var defaultObjects = GenerateDefaultObjects();
        
        var mocks = GetAllMocks();
        
        var expectedResult = Result<UserEmailDto>.Success(defaultObjects.userEmailDto);
        
        var validator = new UserValidator();

        mocks.transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);
        
        mocks.transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        mocks.userRepositoryMock.Setup(u => u.FindAnEmailAsync(It.IsAny<string>()))
                          .ReturnsAsync(false);

        mocks.bcryptUtilMock.Setup(b => b.HashPassword(defaultObjects.createUserDto.Password, 12))
                      .Returns("hashedPassword");

        mocks.userRepositoryMock.Setup(u => u.BeginTransactionAsync())
                          .ReturnsAsync(mocks.transactionMock.Object);

        mocks.userRepositoryMock.Setup(u => u.InsertUser(It.IsAny<UserEntity>()))
                          .Returns(defaultObjects.userEntity);

        mocks.userRepositoryMock.Setup(u => u.SaveChangesAsync())
                          .ReturnsAsync(1);

        mocks.tokenManagerMock.Setup(t => t.GenerateEmailConfirmationToken(defaultObjects.userIdAndEmailDto))
                        .Returns(defaultObjects.emailConfirmationTokenEntity.Token);

        mocks.userRepositoryMock.Setup(u => u.InsertEmailConfirmationToken(It.IsAny<EmailConfirmationTokenEntity>()))
                          .Returns(defaultObjects.emailConfirmationTokenEntity);

        var userService = new UserService(mocks.userRepositoryMock.Object, validator, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        // Act
        var result = await userService.CreateUserAsync(defaultObjects.createUserDto);

        // Assert
        result.Should().BeOfType<Result<UserEmailDto>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();

        // Verify interactions
        mocks.transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mocks.transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(defaultObjects.createUserDto.Email), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.HashPassword(defaultObjects.createUserDto.Password, 12), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
        mocks.tokenManagerMock.Verify(t => t.GenerateEmailConfirmationToken(defaultObjects.userIdAndEmailDto), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.InsertEmailConfirmationToken(It.IsAny<EmailConfirmationTokenEntity>()), Times.Once);
    }

    [Fact(DisplayName = "Given the user and token creation service, when the user data is invalid, then the service should return a validation error.")]
    public async Task CreateUserTest_invalidUser()
    {
        
        // Arrange
        var mocks = GetAllMocks();

        List<IErrorMessage> errorMessages = [new ErrorMessage("Error message", "Any Action")];
        var expectedResult = Result<UserEmailDto>.Failure(new Error(
            (byte)ErrorCodes.InvalidInput,
            (byte)ErrorTypes.InputValidationError,
                  errorMessages));

        var validator = new UserValidator();

        var userService = new UserService(mocks.userRepositoryMock.Object, validator, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        var invalidCreateUserDto = new CreateUserDto("", "test1", "123123");
        
        // Act
        var result = await userService.CreateUserAsync(invalidCreateUserDto);

        // Assert
        result.Should().BeOfType<Result<UserEmailDto>>();
        result.IsSuccess.Should().BeFalse();

        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Never());
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<string>()), Times.Never());
        mocks.userRepositoryMock.Verify(u => u.InsertEmailConfirmationToken(It.IsAny<EmailConfirmationTokenEntity>()), Times.Never());
        mocks.transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);

    }
}