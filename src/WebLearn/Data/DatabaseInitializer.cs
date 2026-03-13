using Dapper;
using Microsoft.Data.Sqlite;

namespace WebLearn.Data;

public class DatabaseInitializer
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IWebHostEnvironment _env;

    public DatabaseInitializer(DbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger, IWebHostEnvironment env)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _env = env;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Bootstrap the migration history table first (001)
        var bootstrapSql = await ReadMigrationScriptAsync("001_CreateMigrationHistory.sql");
        await connection.ExecuteAsync(bootstrapSql);

        // Discover all migration scripts
        var migrationsPath = Path.Combine(_env.ContentRootPath, "Data", "Migrations");
        var scripts = Directory.GetFiles(migrationsPath, "*.sql")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        foreach (var scriptPath in scripts)
        {
            var scriptName = Path.GetFileName(scriptPath);

            var alreadyApplied = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM __MigrationHistory WHERE ScriptName = @ScriptName",
                new { ScriptName = scriptName });

            if (alreadyApplied > 0)
                continue;

            _logger.LogInformation("Applying migration: {ScriptName}", scriptName);

            // For the seed script, we substitute the BCrypt hash at runtime
            var sql = await File.ReadAllTextAsync(scriptPath);
            if (scriptName == "007_SeedData.sql")
                sql = sql.Replace("{{ADMIN_HASH}}", BCrypt.Net.BCrypt.HashPassword("Admin123!"));

            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(sql, transaction: transaction);
                await connection.ExecuteAsync(
                    "INSERT INTO __MigrationHistory (ScriptName, AppliedOn) VALUES (@ScriptName, @AppliedOn)",
                    new { ScriptName = scriptName, AppliedOn = DateTime.UtcNow.ToString("o") },
                    transaction);
                transaction.Commit();
                _logger.LogInformation("Applied migration: {ScriptName}", scriptName);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Failed to apply migration: {ScriptName}", scriptName);
                throw;
            }
        }
    }

    private async Task<string> ReadMigrationScriptAsync(string scriptName)
    {
        var path = Path.Combine(_env.ContentRootPath, "Data", "Migrations", scriptName);
        return await File.ReadAllTextAsync(path);
    }
}
