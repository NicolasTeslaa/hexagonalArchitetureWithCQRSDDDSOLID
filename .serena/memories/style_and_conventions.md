# Style and conventions

## Naming and organization
- Namespaces map closely to folders and layers (`API`, `Application`, `Domain`, `Data.MySql`, `Payment.Application`).
- Interfaces use the `I*` convention (`IBookManager`, `IBookRepository`, `IPaymentProcessor`).
- Application services are named `*Manager` and live under `Application/.../Services`.
- Repository interfaces are in domain `Ports`; implementations are in adapter `Repositories`.
- DTOs and requests/responses are separated by feature folders (`Book`, `Guest`, `Room`, `Payment`).

## Coding style
- Uses standard C# classes/properties, not records.
- Constructor injection is the default pattern.
- Async `Task` methods are used across API, services, and repositories.
- Expression-bodied members are used occasionally for short methods/constructors.
- Domain entities contain business rules directly (for example `Booking.Save` and `Booking.ChangeState`).
- DTOs contain static mapping helpers such as `MapToEntity` / `MapFromEntity`.

## Error handling
- Domain and application logic rely on custom exceptions for validation and business-rule failures.
- Application services translate exceptions into typed response objects with `Success`, `Message`, and `ErrorCode`.
- Controllers map `ErrorCode` values to HTTP responses and log unexpected exceptions with `ILogger`.

## Testing conventions
- Test stack is xUnit with `Fact` / `Theory`, plus Moq.
- Tests are organized by architectural layer.
- Test naming is descriptive and long, often using Portuguese (`Deve_*`, `Quando_*`) and sometimes English (`Should_*`).
- Arrange/Act/Assert comments are used in many tests.

## Notable consistency notes
- The codebase mixes Portuguese and English in messages, exception texts, and test names.
- There is no visible repository-wide formatter or `.editorconfig`; follow the existing local formatting/style in each file.