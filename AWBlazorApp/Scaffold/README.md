# Scaffold

**Generated code. Don't hand-edit.**

If you need to change a file under `Scaffold/`, re-run the scaffold command
that produced it. Today this folder houses the ASP.NET Core Identity UI
scaffold (`Scaffold/Identity/`). Future scaffolders (`dotnet scaffold`
output, OpenAPI client gen, etc.) go here too, each in their own subfolder.

Pages under `Scaffold/Identity/` keep their existing `@page` routes
(`/Account/Login`, `/Account/Register`, `/Account/Manage/*`). That contract
is public and mustn't change because of our folder layout.
