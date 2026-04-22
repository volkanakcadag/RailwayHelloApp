using Npgsql;
using RailwayHelloApp.Models;

namespace RailwayHelloApp.Services;

public sealed class TestRecordRepository(NpgsqlDataSource dataSource)
{
    public async Task<DatabaseConnectionResult> TestConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("select now() at time zone 'utc';", connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            return new DatabaseConnectionResult
            {
                Success = true,
                Message = "PostgreSQL baglantisi basarili.",
                ServerTimeUtc = Convert.ToDateTime(result).ToString("O")
            };
        }
        catch (Exception ex)
        {
            return new DatabaseConnectionResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<IReadOnlyList<TestRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        var items = new List<TestRecord>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(@"
select id, title, category, quantity, unit_price, is_active, created_at
from app_test_records
order by id;", connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapRecord(reader));
        }

        return items;
    }

    public async Task<TestRecord?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(@"
select id, title, category, quantity, unit_price, is_active, created_at
from app_test_records
where id = @id;", connection);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapRecord(reader) : null;
    }

    public async Task<TestRecord> InsertAsync(SaveTestRecordRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(@"
insert into app_test_records (title, category, quantity, unit_price, is_active)
values (@title, @category, @quantity, @unitPrice, @isActive)
returning id, title, category, quantity, unit_price, is_active, created_at;", connection, transaction);

        AddParameters(command, request);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        var created = MapRecord(reader);
        await reader.CloseAsync();

        await transaction.CommitAsync(cancellationToken);
        return created;
    }

    public async Task<TestRecord?> UpdateAsync(int id, SaveTestRecordRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(@"
update app_test_records
set title = @title,
    category = @category,
    quantity = @quantity,
    unit_price = @unitPrice,
    is_active = @isActive
where id = @id
returning id, title, category, quantity, unit_price, is_active, created_at;", connection, transaction);

        command.Parameters.AddWithValue("id", id);
        AddParameters(command, request);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            await reader.CloseAsync();
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var updated = MapRecord(reader);
        await reader.CloseAsync();

        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("delete from app_test_records where id = @id;", connection, transaction);
        command.Parameters.AddWithValue("id", id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static void AddParameters(NpgsqlCommand command, SaveTestRecordRequest request)
    {
        command.Parameters.AddWithValue("title", request.Title);
        command.Parameters.AddWithValue("category", request.Category);
        command.Parameters.AddWithValue("quantity", request.Quantity);
        command.Parameters.AddWithValue("unitPrice", request.UnitPrice);
        command.Parameters.AddWithValue("isActive", request.IsActive);
    }

    private static TestRecord MapRecord(NpgsqlDataReader reader)
    {
        return new TestRecord
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Category = reader.GetString(2),
            Quantity = reader.GetInt32(3),
            UnitPrice = reader.GetDecimal(4),
            IsActive = reader.GetBoolean(5),
            CreatedAt = reader.GetDateTime(6)
        };
    }
}
