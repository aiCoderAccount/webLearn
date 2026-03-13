using Dapper;
using WebLearn.Data;
using WebLearn.Models;
using WebLearn.Repositories.Interfaces;

namespace WebLearn.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly DbConnectionFactory _db;

    public CourseRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Course>> GetAllPublishedAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Course>(
            @"SELECT c.*, i.DisplayName AS InstructorName
              FROM Courses c JOIN Instructors i ON c.InstructorId = i.Id
              WHERE c.IsPublished = 1
              ORDER BY c.CreatedAt DESC");
    }

    public async Task<IEnumerable<Course>> GetByInstructorIdAsync(int instructorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Course>(
            "SELECT * FROM Courses WHERE InstructorId = @InstructorId ORDER BY CreatedAt DESC",
            new { InstructorId = instructorId });
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Course>(
            @"SELECT c.*, i.DisplayName AS InstructorName
              FROM Courses c JOIN Instructors i ON c.InstructorId = i.Id
              WHERE c.Id = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(Course course)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Courses (Title, Description, InstructorId, IsPublished, CreatedAt, UpdatedAt)
              VALUES (@Title, @Description, @InstructorId, @IsPublished, @CreatedAt, @UpdatedAt);
              SELECT LAST_INSERT_ID();",
            course);
    }

    public async Task UpdateAsync(Course course)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE Courses SET Title = @Title, Description = @Description,
              IsPublished = @IsPublished, UpdatedAt = @UpdatedAt
              WHERE Id = @Id",
            course);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Courses WHERE Id = @Id", new { Id = id });
    }
}
