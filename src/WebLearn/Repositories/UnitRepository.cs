using Dapper;
using WebLearn.Data;
using WebLearn.Models;
using WebLearn.Repositories.Interfaces;

namespace WebLearn.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly DbConnectionFactory _db;

    public UnitRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Unit>> GetByCourseIdAsync(int courseId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Unit>(
            "SELECT * FROM Units WHERE CourseId = @CourseId ORDER BY SortOrder",
            new { CourseId = courseId });
    }

    public async Task<Unit?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Unit>(
            "SELECT * FROM Units WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Unit unit)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Units (CourseId, Title, Description, SortOrder, CreatedAt, UpdatedAt)
              VALUES (@CourseId, @Title, @Description, @SortOrder, @CreatedAt, @UpdatedAt);
              SELECT last_insert_rowid();",
            unit);
    }

    public async Task UpdateAsync(Unit unit)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE Units SET Title = @Title, Description = @Description,
              SortOrder = @SortOrder, UpdatedAt = @UpdatedAt
              WHERE Id = @Id",
            unit);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Units WHERE Id = @Id", new { Id = id });
    }
}
