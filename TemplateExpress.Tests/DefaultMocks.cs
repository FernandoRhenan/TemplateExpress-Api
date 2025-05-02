using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Utils;

namespace TemplateExpress.Tests;

internal static class DefaultMocks
{
    
   
   internal static (
       Mock<IUserRepository> userRepositoryMock,
       Mock<IBCryptUtil> bcryptUtilMock,
       Mock<ITokenManager> tokenManagerMock,
       Mock<IDbContextTransaction> transactionMock,
       Mock<TokenValidationResult> tokenValidationResultMock,
       Mock<IEmailSender>emailSenderMock,
       Mock<SecurityToken> securityTokenMock
       ) GetAllMocks()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var bcryptUtilMock = new Mock<IBCryptUtil>();
        var tokenManagerMock = new Mock<ITokenManager>();
        var transactionMock = new Mock<IDbContextTransaction>();
        var tokenValidationResultMock = new Mock<TokenValidationResult>();
        var emailSenderMock = new Mock<IEmailSender>();
        var securityTokenMock = new Mock<SecurityToken>();

        return (userRepositoryMock, bcryptUtilMock, tokenManagerMock, transactionMock, tokenValidationResultMock, emailSenderMock, securityTokenMock);
    }
    
}