using WebLearn.Data;

namespace WebLearn.Repositories;

public abstract class BaseRepository(DbConnectionFactory db)
{
    protected readonly DbConnectionFactory _db = db;
}
