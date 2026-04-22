using Npgsql;

namespace RailwayHelloApp.Infrastructure;

public static class DatabaseConfig
{
    public static NpgsqlDataSource CreateDataSource()
    {
        var connectionString = BuildConnectionString();
        return NpgsqlDataSource.Create(connectionString);
    }

    private static string BuildConnectionString()
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return FromDatabaseUrl(databaseUrl);
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = GetRequired("DB_HOST"),
            Port = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var port) ? port : 5432,
            Database = GetRequired("DB_NAME"),
            Username = GetRequired("DB_USER"),
            Password = GetRequired("DB_PASSWORD"),
            SslMode = ParseSslMode(Environment.GetEnvironmentVariable("DB_SSL_MODE"))
        };

        return builder.ConnectionString;
    }

    private static string FromDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty),
            Password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty),
            SslMode = ParseSslMode(Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "Require")
        };

        return builder.ConnectionString;
    }

    private static string GetRequired(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"'{key}' environment variable bulunamadı. .env dosyasını doldurup uygulamayı tekrar başlatın.");
        }

        return value;
    }

    private static SslMode ParseSslMode(string? value)
    {
        return Enum.TryParse<SslMode>(value, ignoreCase: true, out var sslMode)
            ? sslMode
            : SslMode.Prefer;
    }
}
