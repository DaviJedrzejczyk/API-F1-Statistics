using Dao;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Infrastructure;

public sealed class TestDatabase : IAsyncDisposable
{
    private readonly string name = "R02_Integration_" + Guid.NewGuid().ToString("N");
    private bool created;
    public string ConnectionString { get; }

    public TestDatabase()
    {
        var source = Environment.GetEnvironmentVariable("R02_SQL_CONNECTION")
            ?? throw new InvalidOperationException("Set R02_SQL_CONNECTION to an isolated test SQL Server (Initial Catalog=master). No production fallback is allowed.");
        var builder = new SqlConnectionStringBuilder(source);
        if (builder.InitialCatalog != "master" || !string.IsNullOrEmpty(builder.AttachDBFilename))
            throw new InvalidOperationException("Test SQL connection must select master and must not attach a database file.");
        builder.InitialCatalog = name;
        builder.ConnectTimeout = 3;
        builder.Pooling = false;
        ConnectionString = builder.ConnectionString;
    }

    public ApiF1DB Open() => new(new DbContextOptionsBuilder<ApiF1DB>()
        .UseSqlServer(ConnectionString, sql => sql.CommandTimeout(60)).Options);

    public async Task CreateAsync()
    {
        var master = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        while (true)
        {
            try
            {
                await using var connection = new SqlConnection(master.ConnectionString);
                await connection.OpenAsync(timeout.Token);
                await using var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE [{name}]";
                await command.ExecuteNonQueryAsync(timeout.Token);
                created = true;
                break;
            }
            catch (SqlException) when (!timeout.IsCancellationRequested)
            {
                await Task.Delay(1000, timeout.Token);
            }
        }
        await using var db = Open();
        await db.Database.MigrateAsync(timeout.Token);
    }

    public async ValueTask DisposeAsync()
    {
        if (!created) return;
        // Only this object's generated database can be dropped; never the supplied catalog.
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^R02_Integration_[a-f0-9]{32}$"))
            throw new InvalidOperationException("Unsafe database name.");
        var master = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
        await using var connection = new SqlConnection(master.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]";
        await command.ExecuteNonQueryAsync();
        created = false;
    }
}
