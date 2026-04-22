# RailwayHelloApp

Minimal ASP.NET Core app for Railway deployment.

Requires .NET 10 SDK for local development.

## Run locally

```bash
dotnet restore
dotnet run --no-launch-profile
```

Open:

- http://localhost:8080
- http://localhost:8080/health

## Deploy to Railway

1. Create a new GitHub repository.
2. Upload these files.
3. Push the repository.
4. In Railway, create a new project from the GitHub repo.
5. Railway will build and run the app automatically.

This app listens on `PORT` and binds to `0.0.0.0`, which is required for Railway.
