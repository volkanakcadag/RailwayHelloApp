# RailwayHelloApp

Minimal ASP.NET Core app with PostgreSQL CRUD screen for Railway deployment tests.

Requires .NET 10 SDK for local development.

## Environment

Fill the `.env` file with your PostgreSQL values:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=railway_hello_app
DB_USER=postgres
DB_PASSWORD=postgres
DB_SSL_MODE=Disable
```

## SQL scripts

Run these in order:

1. `sql/001_create_app_test_records.sql`
2. `sql/002_seed_app_test_records.sql`

## Run locally

```bash
dotnet restore
dotnet run --no-launch-profile
```

Open:

- http://localhost:8080
- http://localhost:8080/health
- http://localhost:8080/api/db-status

## What the app does

- Reads PostgreSQL settings from `.env`
- Connects to PostgreSQL using `Npgsql`
- Lists test records
- Inserts, updates and deletes records through the web UI
- Commits each write operation with a database transaction
