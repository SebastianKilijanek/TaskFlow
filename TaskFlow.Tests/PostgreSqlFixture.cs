using Npgsql;
using Testcontainers.PostgreSql;

namespace TaskFlow.Tests;

public static class PostgreSqlFixture
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private static PostgreSqlContainer? _container;

    public static string CreateDatabase()
    {
        var masterConnectionString = GetContainer().GetConnectionString();
        var databaseName = $"test_{Guid.NewGuid():N}";

        using (var connection = new NpgsqlConnection(masterConnectionString))
        {
            connection.Open();
            using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
            command.ExecuteNonQuery();
        }

        var builder = new NpgsqlConnectionStringBuilder(masterConnectionString)
        {
            Database = databaseName
        };

        return builder.ConnectionString;
    }

    public static void DropDatabase(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres";

        using var connection = new NpgsqlConnection(builder.ConnectionString);
        connection.Open();

        using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", connection);
        drop.ExecuteNonQuery();
    }

    private static PostgreSqlContainer GetContainer()
    {
        if (_container is not null)
            return _container;

        Semaphore.Wait();
        try
        {
            if (_container is null)
            {
                _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

                _container.StartAsync().GetAwaiter().GetResult();
            }

            return _container;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}