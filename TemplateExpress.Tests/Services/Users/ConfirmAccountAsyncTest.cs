using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;
using TemplateExpress.Api.Services;

namespace TemplateExpress.Tests.Services.Users;

public class ConfirmAccountAsyncTest
{
    private static readonly Random Random = new();
    
    private (UserIdAndEmailDto userIdAndEmailDto, JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto) GenerateDefaultObjects()
    {
        var userId = (long)Random.Next(1, 50_000);
        
        var createUserDto = new CreateUserDto("test@test.com", "comusertest1", "=d#OdcA)53?p7$$$Sv_0 ");
        var userIdAndEmailDto = new UserIdAndEmailDto(userId, createUserDto.Email);
        var jwtConfirmationAccountTokenDto = new JwtConfirmationAccountTokenDto("token");

        return (userIdAndEmailDto,jwtConfirmationAccountTokenDto);
    }
    private (Mock<IUserRepository> userRepositoryMock, Mock<IBCryptUtil> bcryptUtilMock, Mock<ITokenManager> tokenManagerMock, Mock<IDbContextTransaction> transactionMock, Mock<TokenValidationResult> tokenValidationResultMock) GetAllMocks()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var bcryptUtilMock = new Mock<IBCryptUtil>();
        var tokenManagerMock = new Mock<ITokenManager>();
        var transactionMock = new Mock<IDbContextTransaction>();
        var tokenValidationResultMock = new Mock<TokenValidationResult>();
        return (userRepositoryMock, bcryptUtilMock, tokenManagerMock, transactionMock, tokenValidationResultMock);
    }

    [Fact(DisplayName = "Given the account confirmation user service, when the token is valid, then return a successResponse.")]
    public async Task Success()
    { 
    // Arrange
    var mocks = GetAllMocks();
    var defaultObjects = GenerateDefaultObjects();

    var expectedResult = Result<string>.Success(String.Empty);
    
    mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()))
        .ReturnsAsync(Result<TokenValidationResult>.Success(mocks.tokenValidationResultMock.Object));

    mocks.tokenManagerMock.Setup(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()))
        .Returns(defaultObjects.userIdAndEmailDto);

    mocks.userRepositoryMock.Setup(u => u.ChangeConfirmedAccountColumnToTrue(defaultObjects.userIdAndEmailDto.Id, true))
        .ReturnsAsync(true);
    
    var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

    // Act
    var result = await userService.ConfirmAccountAsync(defaultObjects.jwtConfirmationAccountTokenDto);

    // Assert
    result.Should().BeOfType<Result<string>>();
    result.Should().BeEquivalentTo(expectedResult);
    result.IsSuccess.Should().BeTrue();
    result.Error.Should().BeNull();
    
    // Verify interactions
    mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()), Times.Once);
    mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Once);
    mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), It.IsAny<bool>()), Times.Once);
    
    }

    [Fact(DisplayName = "Given the account confirmation user service, when the token is invalid, then return a successResponse.")]
    public async Task InvalidToken()
    {
        // Arrange
        var mocks = GetAllMocks();
        var defaultObjects = GenerateDefaultObjects();
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
        var expectedResult = Result<TokenValidationResult>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()))
            .ReturnsAsync(expectedResult);
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        // Act
        var result = await userService.ConfirmAccountAsync(defaultObjects.jwtConfirmationAccountTokenDto);

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be((byte)ErrorCodes.InvalidJwtToken);
        result.Error!.Type.Should().Be((byte)ErrorTypes.Unauthorized);
        
        // Verify interactions
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Never);
        mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);

    }

    [Fact(DisplayName = "Given the account confirmation user service, whether any user exists to be confirmed, then return a Exception.")]
    public async Task NoUserExists()
    {
        // Arrange
        var mocks = GetAllMocks();
        var defaultObjects = GenerateDefaultObjects();
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()))
            .ReturnsAsync(Result<TokenValidationResult>.Success(mocks.tokenValidationResultMock.Object));
        
        mocks.tokenManagerMock.Setup(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()))
            .Returns(defaultObjects.userIdAndEmailDto);

        mocks.userRepositoryMock.Setup(u => u.ChangeConfirmedAccountColumnToTrue(defaultObjects.userIdAndEmailDto.Id, true))
            .ReturnsAsync(false);
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        // Act
        Func<Task> act = async () => await userService.ConfirmAccountAsync(defaultObjects.jwtConfirmationAccountTokenDto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("An error occured while trying to confirm the user.");
        
        // Verify interactions
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), It.IsAny<bool>()), Times.Once);

    }

    

}