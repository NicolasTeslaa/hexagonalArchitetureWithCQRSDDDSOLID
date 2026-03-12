# Task completion checklist

When finishing a change in this project, prefer the following:

1. Run the most targeted automated test project for the layer you changed.
2. If the change affects multiple layers, run the broader solution or related test projects.
3. If persistence code changed, verify EF Core migration/update commands and any MySQL configuration assumptions.
4. If API behavior changed, run the API project and validate Swagger/OpenAPI in development.
5. Preserve the architecture boundaries:
   - controllers stay thin
   - application services orchestrate use cases
   - domain entities keep business rules
   - adapters implement ports/infrastructure concerns
6. Keep response/error-code mappings aligned between application services and controllers.
7. Call out environmental blockers explicitly: in this environment, `dotnet build` / `dotnet test` did not complete with actionable diagnostics even though restore started successfully.

If you need to run the API locally, make sure `ConnectionStrings:MySQLConnection` is present (currently found in `appsettings.Development.json`).