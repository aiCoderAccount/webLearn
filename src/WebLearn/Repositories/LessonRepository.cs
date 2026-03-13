using Dapper;
using WebLearn.Data;
using WebLearn.Models;
using WebLearn.Repositories.Interfaces;

namespace WebLearn.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly DbConnectionFactory _db;

    public LessonRepository(DbConnectionFactory db)
    {
        _db = db;
    }

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
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Lessons (Title, XmlContent, InstructorId, CreatedAt, UpdatedAt)
              VALUES (@Title, @XmlContent, @InstructorId, @CreatedAt, @UpdatedAt);
              SELECT LAST_INSERT_ID();",
            lesson);
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
