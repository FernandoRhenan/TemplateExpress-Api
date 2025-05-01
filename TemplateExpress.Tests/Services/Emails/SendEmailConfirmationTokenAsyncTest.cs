using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;
using TemplateExpress.Api.Services;

namespace TemplateExpress.Tests.Services.Emails;

public class SendEmailConfirmationTokenAsyncTest
{
    private (Mock<ITokenManager> tokenManagerMock, Mock<TokenValidationResult> tokenValidationResultMock, Mock<IEmailSender> emailSenderMock) GetAllMocks()
    {
        var tokenManagerMock = new Mock<ITokenManager>();
        var tokenValidationResultMock = new Mock<TokenValidationResult>();
        var emailSenderMock = new Mock<IEmailSender>();

        return (tokenManagerMock, tokenValidationResultMock, emailSenderMock);
    }
    
    private static readonly Random Random = new();

    private static readonly CreateUserDto CreateUserDto = new ("test@test.com", "comusertest1", "=d#OdcA)53?p7$$$Sv_0 ");

    private static readonly UserIdAndEmailDto UserIdAndEmailDto = new (Random.Next(1, 50_000), CreateUserDto.Email);


    [Fact(DisplayName = "Given the sending email confirmation token, when the token is valid, then return a success response.")]
    public async Task Success()
    {
        // Arrange
        var mocks = GetAllMocks();
        
        var expectedResult = Result<string>.Success(String.Empty);
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()))
            .ReturnsAsync(Result<TokenValidationResult>.Success(mocks.tokenValidationResultMock.Object));

        mocks.tokenManagerMock.Setup(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()))
            .Returns(UserIdAndEmailDto);

        mocks.emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        var emailService = new EmailService(mocks.tokenManagerMock.Object, mocks.emailSenderMock.Object);
        var jwtConfirmationAccountTokenDto = new JwtConfirmationAccountTokenDto("token");
        
        // Act
        var result = await emailService.SendEmailConfirmationTokenAsync(jwtConfirmationAccountTokenDto);

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeEquivalentTo(expectedResult);
        
        // Verify interactions
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Once);
        mocks.emailSenderMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

    }
    
    [Fact(DisplayName = "Given the sending email confirmation token, when the token is invalid, then return a validation error response.")]
    public async Task InvalidToken()
    {
        // Arrange
        var mocks = GetAllMocks();
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
        var errorResponse = Result<TokenValidationResult>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        var expectedResult = Result<string>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()))
            .ReturnsAsync(errorResponse);
        
        var emailService = new EmailService(mocks.tokenManagerMock.Object, mocks.emailSenderMock.Object);
        var jwtConfirmationAccountTokenDto = new JwtConfirmationAccountTokenDto("token");
        
        // Act
        var result = await emailService.SendEmailConfirmationTokenAsync(jwtConfirmationAccountTokenDto);

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be((byte)ErrorCodes.InvalidJwtToken);
        result.Error!.Type.Should().Be((byte)ErrorTypes.Unauthorized);
        result.Error!.Messages.Should().BeOfType<List<IErrorMessage>>();
        
        // Verify interactions
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Never);
        mocks.emailSenderMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

    }
}