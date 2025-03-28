namespace TemplateExpress.Api.Results;

public class Error
{
    public string Code { get; }
    public string Type { get; }
    public List<IErrorMessage> Messages { get; }
    public Error(string code, string type, List<IErrorMessage> messages)
    {
        Code = code;
        Type = type;
        Messages = messages;
    }
    
}

public class ErrorMessage : IErrorMessage
{
    public string Message { get; }
    public string Action { get; }

    public ErrorMessage(string message, string action)
    {
        Message = message;
        Action = action;
    }
}
public interface IErrorMessage
{
    string Message { get; }
    string Action { get; }
}