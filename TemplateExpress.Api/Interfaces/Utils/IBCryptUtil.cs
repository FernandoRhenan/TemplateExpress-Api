namespace TemplateExpress.Api.Interfaces.Utils;

public interface IBCryptUtil
{
    string HashPassword(string password, int workFactor = 12);
    bool ComparePassword(string password, string hashedPassword);
}