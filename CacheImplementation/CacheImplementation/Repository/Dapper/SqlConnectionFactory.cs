using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CacheImplementation.Repository.Dapper;

public sealed class SqlConnectionFactory(IConfiguration configuration): IDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("AzureInventory") ?? throw new InvalidOperationException("Database connection string is not configured.");
    public IDbConnection Create() => new SqlConnection(_connectionString);
}
