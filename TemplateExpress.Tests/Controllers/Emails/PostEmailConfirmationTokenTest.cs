using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TemplateExpress.Api.Controllers;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;

namespace TemplateExpress.Tests.Controllers.Emails;

public class PostEmailConfirmationTokenTest
{
    private static readonly JwtConfirmationAccountTokenDto JwtConfirmationAccountTokenDto = new JwtConfirmationAccountTokenDto("token");

    [Fact(DisplayName = "Given the email token sending, when the email is sent, then return a success")]
    public async Task Success()
    {
        // Arrange
        
        var emailServiceMock = new Mock<IEmailService>();
        
        var emailController = new EmailController(emailServiceMock.Object);
        
        emailServiceMock.Setup(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()))
            .ReturnsAsync(Result<string>.Success(String.Empty));

        // Act
        var result = await emailController.PostEmailConfirmationToken(JwtConfirmationAccountTokenDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify interactions
        emailServiceMock.Verify(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()), Times.Once);

    }   
    
    [Fact(DisplayName = "Given the email token sending, when the email dont is sent, then return a error")]
    public async Task Error()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var emailController = new EmailController(emailServiceMock.Object);
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
        var error = Result<string>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        emailServiceMock.Setup(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()))
            .ReturnsAsync(error);

        // Act
        var result = await emailController.PostEmailConfirmationToken(JwtConfirmationAccountTokenDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        
        // Verify interactions
        emailServiceMock.Verify(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtConfirmationAccountTokenDto>()), Times.Once);

    }
    
}