namespace TemplateExpress.Api.Utils;

public static class BCryptUtil
{
    public static string HashPassword(string password, int workFactor = 10)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    }

    public static bool CompareHash(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}