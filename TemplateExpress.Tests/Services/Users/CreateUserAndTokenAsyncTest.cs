using System.Transactions;
using FluentAssertions;
using Moq;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Services;
using TemplateExpress.Api.Validations.Users;

namespace TemplateExpress.Tests.Services.Users;

public class CreateUserAndTokenAsyncTest
{
    
    [Fact(DisplayName = "Given the user and token creation service, when the user data is valid, then return a success response with the confirmation account token.")]
    public async Task Success()
    {
        // Arrange
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        var mocks = DefaultMocks.GetAllMocks();
        
        var jwtConfirmationAccountToken = new JwtTokenDto("token");
        
        var expectedResult = Result<JwtTokenDto>.Success(jwtConfirmationAccountToken);
        
        var validator = new CreateUserValidator();

        mocks.transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);
        
        mocks.transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        mocks.userRepositoryMock.Setup(u => u.FindAnEmailAsync(It.IsAny<UserEmailDto>()))
                          .ReturnsAsync(false);

        mocks.bcryptUtilMock.Setup(b => b.HashPassword(defaultObjects.createUserDto.Password, 12))
                      .Returns("hashedPassword");

        mocks.userRepositoryMock.Setup(u => u.BeginTransactionAsync())
                          .ReturnsAsync(mocks.transactionMock.Object);

        mocks.userRepositoryMock.Setup(u => u.InsertUser(It.IsAny<UserEntity>()))
                          .Returns(defaultObjects.userEntity);

        mocks.userRepositoryMock.Setup(u => u.SaveChangesAsync())
                          .ReturnsAsync(1);
        
        mocks.tokenManagerMock.Setup(t => t.GenerateAccountConfirmationToken(defaultObjects.userIdAndEmailDto))
            .Returns(jwtConfirmationAccountToken.Token);
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        // Act
        var result = await userService.CreateUserAndTokenAsync(defaultObjects.createUserDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();

        // Verify interactions
        mocks.transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mocks.transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.bcryptUtilMock.Verify(b => b.HashPassword(It.IsAny<string>(), 12), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(1));
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Once);
    }

    [Fact(DisplayName = "Given the user and token creation service, when the user data is invalid, then return a validation error.")]
    public async Task InvalidUser()
    {
        
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();

        var validator = new CreateUserValidator();

        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        var invalidCreateUserDto = new CreateUserDto("", "test1", "123123");
        
        // Act
        var result = await userService.CreateUserAndTokenAsync(invalidCreateUserDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be((byte)ErrorCodes.InvalidInput);
        result.Error?.Type.Should().Be((byte)ErrorTypes.InputValidationError);
        result.Error?.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Never());
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<UserEmailDto>()), Times.Never());
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Never);

    }

    [Fact(DisplayName = "Given the user and token creation service, when the user already exists, then return a emailAlreadyExists error.")]
    public async Task EmailAlreadyExists()
    {
        
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();

        var validator = new CreateUserValidator();
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("This email is already in use.", "Try another email.")];

        mocks.userRepositoryMock.Setup(u => u.FindAnEmailAsync(It.IsAny<UserEmailDto>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await userService.CreateUserAndTokenAsync(defaultObjects.createUserDto, validator);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error?.Code.Should().Be((byte)ErrorCodes.EmailAlreadyExists);
        result.Error?.Type.Should().Be((byte)ErrorTypes.BusinessLogicValidationError);
        result.Error?.Messages.Should().BeOfType<List<IErrorMessage>>();
        result.Error?.Messages.Should().BeEquivalentTo(errorMessages);
        
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Never);
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Never);

    }

    [Fact(DisplayName = "Given the user and token creation service, when occurs a error in transaction, then throw a TransactionException.")]
    public async Task TransactionException()
    {
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();

        var validator = new CreateUserValidator();
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object,
            mocks.tokenManagerMock.Object);

        mocks.userRepositoryMock.Setup(u => u.FindAnEmailAsync(It.IsAny<UserEmailDto>()))
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
        Func<Task> act = async () => await userService.CreateUserAndTokenAsync(defaultObjects.createUserDto, validator);

        // Assert
        await act.Should().ThrowAsync<TransactionException>().WithMessage("An error occured while trying to create the user.");
        
        //Verify interactions
        mocks.userRepositoryMock.Verify(u => u.FindAnEmailAsync(It.IsAny<UserEmailDto>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.InsertUser(It.IsAny<UserEntity>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        mocks.transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mocks.transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GenerateAccountConfirmationToken(It.IsAny<UserIdAndEmailDto>()), Times.Never);

    }
}