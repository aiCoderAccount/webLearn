using Dapper;
using WebLearn.Data;
using WebLearn.Models;
using WebLearn.Repositories.Interfaces;

namespace WebLearn.Repositories;

public class UnitLessonRepository : IUnitLessonRepository
{
    private readonly DbConnectionFactory _db;

    public UnitLessonRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<UnitLesson>> GetByUnitIdAsync(int unitId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<UnitLesson>(
            @"SELECT ul.*, l.Title AS LessonTitle
              FROM UnitLessons ul JOIN Lessons l ON ul.LessonId = l.Id
              WHERE ul.UnitId = @UnitId
              ORDER BY ul.SortOrder",
            new { UnitId = unitId });
    }

    public async Task AssignAsync(int unitId, int lessonId, int sortOrder)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT OR IGNORE INTO UnitLessons (UnitId, LessonId, SortOrder)
              VALUES (@UnitId, @LessonId, @SortOrder)",
            new { UnitId = unitId, LessonId = lessonId, SortOrder = sortOrder });
    }

    public async Task UnassignAsync(int unitId, int lessonId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM UnitLessons WHERE UnitId = @UnitId AND LessonId = @LessonId",
            new { UnitId = unitId, LessonId = lessonId });
    }

    public async Task UpdateSortOrderAsync(int unitId, int lessonId, int sortOrder)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE UnitLessons SET SortOrder = @SortOrder
              WHERE UnitId = @UnitId AND LessonId = @LessonId",
            new { UnitId = unitId, LessonId = lessonId, SortOrder = sortOrder });
    }
}
