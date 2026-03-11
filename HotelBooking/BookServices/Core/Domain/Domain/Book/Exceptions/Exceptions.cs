using Domain.Book.Enums;

namespace Domain.Book.Exceptions;

public class InvalidBookingDateRangeException : Exception
{
    public InvalidBookingDateRangeException()
        : base("Booking end date must be greater than start date.")
    {
    }
}

public class BookingInPastException : Exception
{
    public BookingInPastException()
        : base("Booking start date cannot be in the past.")
    {
    }
}

public class RoomRequiredException : Exception
{
    public RoomRequiredException()
        : base("Room is required.")
    {
    }
}

public class GuestRequiredException : Exception
{
    public GuestRequiredException()
        : base("Guest is required.")
    {
    }
}

public class RoomUnavailableException : Exception
{
    public RoomUnavailableException()
        : base("Room is unavailable for booking.")
    {
    }
}
public class BookingConflictException : Exception
{
    public BookingConflictException()
        : base("There is already a booking for this room in the selected period.")
    {
    }
}


public class InvalidBookingStateTransitionException : Exception
{
    public InvalidBookingStateTransitionException(Status currentStatus, Enums.Action action)
        : base($"Cannot execute action '{action}' when booking is in status '{currentStatus}'.")
    {
    }
}