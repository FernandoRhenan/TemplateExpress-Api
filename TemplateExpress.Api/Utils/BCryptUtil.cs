using TemplateExpress.Api.Interfaces.Utils;

namespace TemplateExpress.Api.Utils;

public class BCryptUtil : IBCryptUtil
{
    public string HashPassword(string password, int workFactor = 12)
    => BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    

    public bool ComparePassword(string password, string hashedPassword)
    => BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    
}
