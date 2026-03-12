# Suggested commands

## Windows shell basics
- List files: `Get-ChildItem`
- List files recursively: `Get-ChildItem -Recurse`
- Change directory: `Set-Location <path>`
- Search text: `rg "pattern"`
- Search files by name: `rg --files | rg "name"`
- Git status: `git status`

## .NET solution commands
Run from repo root `C:\repo\hexagonalArchitetureWithCQRSDDDSOLID`.

- List solution projects: `dotnet sln .\HotelBooking\HotelBooking.slnx list`
- Build solution: `dotnet build .\HotelBooking\HotelBooking.slnx -nologo`
- Build API only: `dotnet build .\HotelBooking\BookServices\Consumers\API\API.csproj -nologo`
- Run API: `dotnet run --project .\HotelBooking\BookServices\Consumers\API\API.csproj`
- Run all tests in solution: `dotnet test .\HotelBooking\HotelBooking.slnx -nologo`
- Run domain tests only: `dotnet test .\HotelBooking\BookServices\Tests\Core\Domain\DomainTests\DomainTests.csproj -nologo`
- Run API tests only: `dotnet test .\HotelBooking\BookServices\Tests\Consumers\APITests\APITests.csproj -nologo`

## EF Core / database
Migration example documented in `HotelBooking/ExemploMigration.txt`:
- `dotnet ef migrations add <MigrationName> --project .\HotelBooking\BookServices\Adapters\Data.MySql\Data.MySql.csproj --startup-project .\HotelBooking\BookServices\Consumers\API\API.csproj`
- `dotnet ef database update --project .\HotelBooking\BookServices\Adapters\Data.MySql\Data.MySql.csproj --startup-project .\HotelBooking\BookServices\Consumers\API\API.csproj`

## Environment note
I was able to validate `dotnet sln .\HotelBooking\HotelBooking.slnx list`. In this environment, `dotnet build` / `dotnet test` returned failure or stalled after restore without surfacing compiler errors, so treat the commands above as the intended workflow but re-validate locally when needed.