using Dapper;
using WebLearn.Data;
using WebLearn.Models;
using WebLearn.Repositories.Interfaces;

namespace WebLearn.Repositories;

public class InstructorRepository(DbConnectionFactory db) : BaseRepository(db), IInstructorRepository
{
    public async Task<Instructor?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Instructor>(
            "SELECT * FROM Instructors WHERE Id = @Id", new { Id = id });
    }

    public async Task<Instructor?> GetByUsernameAsync(string username)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Instructor>(
            "SELECT * FROM Instructors WHERE Username = @Username", new { Username = username });
    }

    public async Task<int> CreateAsync(Instructor instructor)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO Instructors (Username, PasswordHash, DisplayName, Email, CreatedAt, UpdatedAt)
              VALUES (@Username, @PasswordHash, @DisplayName, @Email, @CreatedAt, @UpdatedAt);",
            instructor);
        return await conn.ExecuteScalarAsync<int>("SELECT last_insert_rowid();");
    }

    public async Task UpdateAsync(Instructor instructor)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE Instructors SET Username = @Username, PasswordHash = @PasswordHash,
              DisplayName = @DisplayName, Email = @Email, UpdatedAt = @UpdatedAt
              WHERE Id = @Id",
            instructor);
    }
}
