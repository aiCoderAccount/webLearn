using Dapper;
using WebLearn.Data;
using WebLearn.Models;
using WebLearn.Repositories.Interfaces;

namespace WebLearn.Repositories;

public class LessonRepository(DbConnectionFactory db) : BaseRepository(db), ILessonRepository
{
    public async Task<IEnumerable<Lesson>> GetByInstructorIdAsync(int instructorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Lesson>(
            "SELECT * FROM Lessons WHERE InstructorId = @InstructorId ORDER BY CreatedAt DESC",
            new { InstructorId = instructorId });
    }

    public async Task<Lesson?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Lesson>(
            "SELECT * FROM Lessons WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Lesson lesson)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO Lessons (Title, XmlContent, InstructorId, CreatedAt, UpdatedAt)
              VALUES (@Title, @XmlContent, @InstructorId, @CreatedAt, @UpdatedAt);",
            lesson);
        return await conn.ExecuteScalarAsync<int>("SELECT last_insert_rowid();");
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE Lessons SET Title = @Title, XmlContent = @XmlContent,
              UpdatedAt = @UpdatedAt WHERE Id = @Id",
            lesson);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Lessons WHERE Id = @Id", new { Id = id });
    }
}
