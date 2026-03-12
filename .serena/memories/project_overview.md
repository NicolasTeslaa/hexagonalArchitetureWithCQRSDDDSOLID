# Project overview

## Purpose
This repository contains a hotel booking backend organized around a hexagonal architecture with DDD-inspired boundaries. The main implemented flow is `BookServices`, which exposes an ASP.NET Core API for guests, rooms, bookings, and payment initiation. There is also a `PaymentServices` application module with payment-provider adapters and a factory used by the booking application.

## Architecture
The solution structure reflects ports-and-adapters boundaries:
- `BookServices/Consumers/API`: HTTP entrypoint with controllers, DI, and configuration.
- `BookServices/Core/Application/Application`: application services (`*Manager`), DTOs, requests, responses, and ports.
- `BookServices/Core/Domain/Domain`: domain entities, value objects, domain exceptions, enums, and repository ports.
- `BookServices/Adapters/Data.MySql`: EF Core/MySQL persistence adapter and repository implementations.
- `BookServices/Tests`: tests grouped by adapter, API, application, and domain layers.
- `PaymentServices/Core/Application/Application`: payment adapters (`MercadoPago`, `Paypal`, `Stripe`) and `PaymentProcessorFactory`.

## Tech stack
- C# / .NET 9 (`net9.0`)
- ASP.NET Core Web API
- Entity Framework Core 9
- Pomelo MySQL provider for EF Core
- Swagger / OpenAPI
- xUnit + Moq + coverlet for tests

## Solution notes
- Main solution file: `HotelBooking/HotelBooking.slnx`
- Main runnable entrypoint found: `HotelBooking/BookServices/Consumers/API/API.csproj`
- Development connection string is stored in `BookServices/Consumers/API/appsettings.Development.json` under `ConnectionStrings:MySQLConnection`.
- `PaymentServices` currently contains application-layer code only; no runnable API project was found for it.

## Implementation caveat
The repository name mentions CQRS/SOLID, but the current codebase is more clearly organized around layered hexagonal boundaries plus DDD-style entities/ports than explicit CQRS handlers or MediatR-style command/query separation.