using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Security;

namespace TemplateExpress.Tests.Security;

public class TokenManagerTests
{
    [Fact(DisplayName = "Given the Email Confirmation Token Generation, when JWT secret is provided, then returns the token successfully")]
    public void GenerateEmailConfirmationTokenAsync_ValidUserAndSecret()
    {
        // Arrange
        IOptions<JwtOptions> jwtOptions = Options.Create(new JwtOptions());
        jwtOptions.Value.Secret = "testSecretKeyThatHaveMoreThan128Bits";
        var userIdAndEmailDto = new UserIdAndEmailDto(1, "test@test.com");
        var tokenManagerSecurity = new TokenManager(jwtOptions);
        
        // Act
        var token = tokenManagerSecurity.GenerateEmailConfirmationTokenAsync(userIdAndEmailDto);
        
        // Assert
        var handler = new JwtSecurityTokenHandler();
        var canReadToken = handler.CanReadToken(token);
        var readToken = handler.ReadJwtToken(token);
        
        canReadToken.Should().BeTrue();
        readToken.Claims.Count().Should().Be(5);
        
        var listOfClaimTypes = readToken.Claims.Select(c => c.Type).ToList();
        
        listOfClaimTypes.Contains("email").Should().BeTrue();
        listOfClaimTypes.Contains("unique_name").Should().BeTrue();
        listOfClaimTypes.Contains("iat").Should().BeTrue();
        listOfClaimTypes.Contains("exp").Should().BeTrue();
        listOfClaimTypes.Contains("nbf").Should().BeTrue();
        
        foreach (Claim claim in readToken.Claims)
        {
            if(claim.Type == "email") claim.Value.Should().Be(userIdAndEmailDto.Email); 
            if(claim.Type == "unique_name") claim.Value.Should().Be(userIdAndEmailDto.Id.ToString()); 
        }
        
        var issuedAt = readToken.ValidFrom;
        var expiresAt = readToken.ValidTo;
        TimeSpan difference = expiresAt - issuedAt;
        difference.TotalMinutes.Should().Be(720); // 12H
    }

    [Fact(DisplayName = "Given the Email Confirmation Token Generation, when JWT secret is not provided, then returns an exception.")]
    public void GenerateEmailConfirmationTokenAsync_ValidUserAndInvalidSecret()
    {
        // Arrange
        IOptions<JwtOptions> jwtOptions = Options.Create(new JwtOptions());
        jwtOptions.Value.Secret = "";
        var userIdAndEmailDto = new UserIdAndEmailDto(1, "test@test.com");
        var tokenManagerSecurity = new TokenManager(jwtOptions);
        
        // Act
        Action act = () => tokenManagerSecurity.GenerateEmailConfirmationTokenAsync(userIdAndEmailDto);
        
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Missing JWT secret.");
        
    }
}