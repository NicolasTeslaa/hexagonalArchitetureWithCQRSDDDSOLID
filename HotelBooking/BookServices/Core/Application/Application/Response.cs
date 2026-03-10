namespace Application;

public enum ErrorCodes
{
    NOT_FOUND = 0,
    COULD_NOT_STORE_DATA = 1,
    INVALID_PERSON_DOCUMENT_ID = 2,
    MISSING_REQUIRED_INFORMATION = 3,
    INVALID_EMAIL = 4,
    UNEXPECTED_ERROR = 5
}

public abstract class Response
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ErrorCodes ErrorCode { get; set; }
}
