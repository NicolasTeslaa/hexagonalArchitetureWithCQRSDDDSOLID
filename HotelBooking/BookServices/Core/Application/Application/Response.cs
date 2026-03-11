namespace Application;

public enum ErrorCodes
{
    NOT_FOUND = 0,
    COULD_NOT_STORE_DATA = 1,
    INVALID_PERSON_DOCUMENT_ID = 2,
    MISSING_REQUIRED_INFORMATION = 3,
    INVALID_EMAIL = 4,
    UNEXPECTED_ERROR = 5,

    //book
    ROOM_REQUIRED = 6,
    GUEST_REQUIRED = 7,
    INVALID_BOOKING_DATE_RANGE = 8,
    BOOKING_IN_PAST = 9,
    ROOM_UNAVAILABLE = 10,
    BOOKING_CONFLICT = 11,
    INVALID_BOOKING_STATE_TRANSITION = 12
}

public abstract class Response
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ErrorCodes ErrorCode { get; set; }
}
