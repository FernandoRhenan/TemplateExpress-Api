using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Services;

namespace TemplateExpress.Tests.Services.Users;

public class ConfirmAccountAsyncTest
{

    [Fact(DisplayName = "Given the account confirmation user service, when the token is valid, then return a success response.")]
    public async Task Success()
    { 
    // Arrange
    var mocks = DefaultMocks.GetAllMocks();
    var defaultObjects = DefaultObjects.GenerateDefaultObjects();

    var authToken = "authToken";
    
    mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()))
        .ReturnsAsync(Result<TokenValidationResult>.Success(mocks.tokenValidationResultMock.Object));

    mocks.tokenManagerMock.Setup(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()))
        .Returns(defaultObjects.userIdAndEmailDto);

    mocks.userRepositoryMock.Setup(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), true))
        .ReturnsAsync(defaultObjects.userEntity);

    mocks.tokenManagerMock.Setup(t => t.GenerateAuthenticationToken(It.IsAny<UserIdAndRoleDto>()))
        .Returns(authToken);
    
    var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

    // Act
    var result = await userService.ConfirmAccountAsync(defaultObjects.jwtConfirmationAccountTokenDto);

    // Assert
    result.Should().BeOfType<Result<JwtTokenDto>>();
    result.Value.Should().BeOfType<JwtTokenDto>();
    result.Value.Should().BeEquivalentTo(new JwtTokenDto(authToken));
    result.IsSuccess.Should().BeTrue();
    result.Error.Should().BeNull();
    
    // Verify interactions
    mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()), Times.Once);
    mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Once);
    mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), true), Times.Once);
    mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), false), Times.Never);
    mocks.tokenManagerMock.Verify(t => t.GenerateAuthenticationToken(It.IsAny<UserIdAndRoleDto>()), Times.Once);
    
    }

    [Fact(DisplayName = "Given the account confirmation user service, when the token is invalid, then return a token validation error response.")]
    public async Task InvalidToken()
    {
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
        var expectedResultOfValidation = Result<TokenValidationResult>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(expectedResultOfValidation);
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        var expectedFinalResult = Result<JwtTokenDto>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        // Act
        var result = await userService.ConfirmAccountAsync(defaultObjects.jwtConfirmationAccountTokenDto);

        // Assert
        result.Should().BeOfType<Result<JwtTokenDto>>();
        result.Should().BeEquivalentTo(expectedFinalResult);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be((byte)ErrorCodes.InvalidJwtToken);
        result.Error!.Type.Should().Be((byte)ErrorTypes.Unauthorized);
        
        // Verify interactions
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Never);
        mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);

    }

    [Fact(DisplayName = "Given the account confirmation user service, whether any user exists to be confirmed, then return a Exception.")]
    public async Task NoUserExists()
    {
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(Result<TokenValidationResult>.Success(mocks.tokenValidationResultMock.Object));
        
        mocks.tokenManagerMock.Setup(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()))
            .Returns(defaultObjects.userIdAndEmailDto);

        mocks.userRepositoryMock.Setup(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), true));
        
        var userService = new UserService(mocks.userRepositoryMock.Object, mocks.bcryptUtilMock.Object, mocks.tokenManagerMock.Object);

        // Act
        Func<Task> act = async () => await userService.ConfirmAccountAsync(defaultObjects.jwtConfirmationAccountTokenDto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("An error occured while trying to confirm the user.");
        
        // Verify interactions
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), true), Times.Once);
        mocks.userRepositoryMock.Verify(u => u.ChangeConfirmedAccountColumnToTrue(It.IsAny<long>(), false), Times.Never);

    }

    

}