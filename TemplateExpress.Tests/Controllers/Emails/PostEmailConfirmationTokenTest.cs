using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TemplateExpress.Api.Controllers;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.EnumResponseTypes;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Tests.Controllers.Emails;

public class PostEmailConfirmationTokenTest
{
    private static readonly JwtTokenDto JwtTokenDto = new JwtTokenDto("token");

    [Fact(DisplayName = "Given the email token sending, when the email is sent, then return a success")]
    public async Task Success()
    {
        // Arrange
        
        var emailServiceMock = new Mock<IEmailService>();
        
        var emailController = new EmailController(emailServiceMock.Object);
        
        emailServiceMock.Setup(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(Result<string>.Success(String.Empty));

        // Act
        var result = await emailController.PostEmailConfirmationToken(JwtTokenDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify interactions
        emailServiceMock.Verify(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtTokenDto>()), Times.Once);

    }   
    
    [Fact(DisplayName = "Given the email token sending, when the email dont is sent, then return a error")]
    public async Task Error()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var emailController = new EmailController(emailServiceMock.Object);
        
        List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
        var error = Result<string>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        
        emailServiceMock.Setup(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtTokenDto>()))
            .ReturnsAsync(error);

        // Act
        var result = await emailController.PostEmailConfirmationToken(JwtTokenDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        
        // Verify interactions
        emailServiceMock.Verify(e => e.SendEmailConfirmationTokenAsync(It.IsAny<JwtTokenDto>()), Times.Once);

    }
    
}