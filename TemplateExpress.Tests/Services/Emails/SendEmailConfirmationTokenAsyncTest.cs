using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Services;

namespace TemplateExpress.Tests.Services.Emails;

public class SendEmailConfirmationTokenAsyncTest
{
    
    [Fact(DisplayName = "Given the sending email confirmation token, when the token is valid, then return a success response.")]
    public async Task Success()
    {
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();
        var defaultObjects = DefaultObjects.GenerateDefaultObjects();
        
        var expectedResult = Result<string>.Success(String.Empty);
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(Result<TokenValidationResult>.Success(mocks.tokenValidationResultMock.Object));

        mocks.tokenManagerMock.Setup(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()))
            .Returns(defaultObjects.userIdAndEmailDto);

        mocks.emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        var emailService = new EmailService(mocks.tokenManagerMock.Object, mocks.emailSenderMock.Object);
        var jwtConfirmationAccountTokenDto = new JwtTokenDto("token");
        
        // Act
        var result = await emailService.SendEmailConfirmationTokenAsync(jwtConfirmationAccountTokenDto);

        // Assert
        result.Should().BeOfType<Result<string>>();
        result.Should().BeEquivalentTo(expectedResult);
        
        // Verify interactions
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Once);
        mocks.emailSenderMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

    }
    
    [Fact(DisplayName = "Given the sending email confirmation token, when the token is invalid, then return a validation error response.")]
    public async Task InvalidToken()
    {
        // Arrange
        var mocks = DefaultMocks.GetAllMocks();
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
        var errorResponse = Result<TokenValidationResult>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        var expectedResult = Result<string>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        mocks.tokenManagerMock.Setup(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(errorResponse);
        
        var emailService = new EmailService(mocks.tokenManagerMock.Object, mocks.emailSenderMock.Object);
        var jwtConfirmationAccountTokenDto = new JwtTokenDto("token");
        
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
        mocks.tokenManagerMock.Verify(t => t.ValidateAccountConfirmationTokenAsync(It.IsAny<JwtTokenDto>()), Times.Once);
        mocks.tokenManagerMock.Verify(t => t.GetJwtConfirmationAccountTokenClaims(It.IsAny<TokenValidationResult>()), Times.Never);
        mocks.emailSenderMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

    }
}