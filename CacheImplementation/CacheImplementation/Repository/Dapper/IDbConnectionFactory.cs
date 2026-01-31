using System.Data;

namespace CacheImplementation.Repository.Dapper;
public interface IDbConnectionFactory
{
    IDbConnection Create();
}
