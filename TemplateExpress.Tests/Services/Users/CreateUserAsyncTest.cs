using System.Transactions;
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

namespace TemplateExpress.Tests.Services.Users;

public class CreateUserAsyncTest
{

    private static readonly DateTime Now = DateTime.Now;
    private static readonly Random Random = new();
    
    // TODO: Review this method and check de logic
    private (UserEntity userEntity, CreateUserDto createUserDto, UserIdAndEmailDto userIdAndEmailDto, UserEmailDto userEmailDto) GenerateDefaultObjects()
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

        return (userEntity, createUserDto, userIdAndEmailDto, userEmailDto);
    }

    // TODO: Review this method and check de logic
    private (Mock<IUserRepository> userRepositoryMock, Mock<IBCryptUtil> bcryptUtilMock, Mock<ITokenManager>
        tokenManagerMock, Mock<IDbContextTransaction> transactionMock) GetAllMocks()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var bcryptUtilMock = new Mock<IBCryptUtil>();
        var tokenManagerMock = new Mock<ITokenManager>();
        var transactionMock = new Mock<IDbContextTransaction>();
        return (userRepositoryMock, bcryptUtilMock, tokenManagerMock, transactionMock);
    }
    
    
    // TODO: Review this test and check de logic
    [Fact(DisplayName = "Given the user and token creation service, when the user data is valid, then return a successResponse.")]
    public async Task Success()
    {
        // Arrange
        var defaultObjects = GenerateDefaultObjects();
        
        var mocks = GetAllMocks();
        
        var jwtConfirmationAccountToken = new JwtConfirmationAccountTokenDto("token");
        
        var expectedResult = Result<JwtConfirmationAccountTokenDto>.Success(jwtConfirmationAccountToken);
        
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
            .Returns("token");
        
        var userService = new UserService(mocks.userRepositoryMock.Object, validator, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        // Act
        var result = await userService.CreateUserAsync(defaultObjects.createUserDto);

        // Assert
        result.Should().BeOfType<Result<JwtConfirmationAccountTokenDto>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();

        // Verify interactions
        mocks.transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mocks.transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<string>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.HashPassword(It.IsAny<string>(), 12), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(1));
        mocks.tokenManagerMock.Verify(t => t.GenerateEmailConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Once);
    }

    // TODO: Review this test and check de logic
    [Fact(DisplayName = "Given the user and token creation service, when the user data is invalid, then return a validation error.")]
    public async Task InvalidUser()
    {
        
        // Arrange
        var mocks = GetAllMocks();

        var validator = new UserValidator();

        var userService = new UserService(mocks.userRepositoryMock.Object, validator, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        var invalidCreateUserDto = new CreateUserDto("", "test1", "123123");
        
        // Act
        var result = await userService.CreateUserAsync(invalidCreateUserDto);

        // Assert
        result.Should().BeOfType<Result<JwtConfirmationAccountTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error?.Type.Should().Be((byte)ErrorTypes.InputValidationError);
        result.Error?.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Never());
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<string>()), Times.Never());
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Never);

    }

    // TODO: Review this test and check de logic
    [Fact(DisplayName = "Given the user and token creation service, when the user already exists, then return a emailAlreadyExists error.")]
    public async Task EmailAlreadyExists()
    {
        
        // Arrange
        var mocks = GetAllMocks();
        var defaultObjects = GenerateDefaultObjects();

        var validator = new UserValidator();
        var userService = new UserService(mocks.userRepositoryMock.Object, validator, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("This email is already in use.", "Try another email.")];

        mocks.userRepositoryMock.Setup(u => u.FindAnEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await userService.CreateUserAsync(defaultObjects.createUserDto);

        // Assert
        result.Should().BeOfType<Result<JwtConfirmationAccountTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be((byte)ErrorCodes.EmailAlreadyExists);
        result.Error?.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        result.Error?.Messages.Should().BeOfType<List<IErrorMessage>>();
        result.Error?.Messages.Should().BeEquivalentTo(errorMessages);
        
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<string>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Never);
        mocks.tokenManagerMock.Verify(t => t.GenerateEmailConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Never);

    }

    // TODO: Review this test and check de logic
    [Fact(DisplayName = "Given the user and token creation service, when occurs a error in transaction, then throw a TransactionException.")]
    public async Task TransactionException()
    {
        // Arrange
        var mocks = GetAllMocks();
        var defaultObjects = GenerateDefaultObjects();

        var validator = new UserValidator();
        var userService = new UserService(mocks.userRepositoryMock.Object, validator, mocks.bcryptUtilMock.Object,
            mocks.tokenManagerMock.Object);

        mocks.userRepositoryMock.Setup(u => u.FindAnEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        mocks.bcryptUtilMock.Setup(b => b.HashPassword(defaultObjects.createUserDto.Password, 12))
            .Returns("hashedPassword");

        mocks.userRepositoryMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(mocks.transactionMock.Object);

        mocks.userRepositoryMock.Setup(u => u.InsertUser(It.IsAny<UserEntity>()))
            .Returns(defaultObjects.userEntity);
        
        mocks.userRepositoryMock.Setup(u => u.InsertUser(It.IsAny<UserEntity>()))
            .Throws(new TransactionException("An error occured while trying to create the user."));


        // Act
        Func<Task> act = async () => await userService.CreateUserAsync(defaultObjects.createUserDto);

        // Assert
        await act.Should().ThrowAsync<TransactionException>().WithMessage("An error occured while trying to create the user.");
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<string>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Once);
        mocks.transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mocks.transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

    }
}