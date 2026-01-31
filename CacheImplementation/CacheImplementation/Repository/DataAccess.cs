using System.Data;
using Dapper;
using CacheImplementation.Model;
using Microsoft.Extensions.Logging;
using CacheImplementation.Repository.Dapper;

namespace CacheImplementation.Repository;

public sealed class DataAccess(
    IDbConnectionFactory factory,
    ILogger<DataAccess> logger) : IDataAccess
{
    private readonly IDbConnectionFactory _factory = factory;
    private readonly ILogger<DataAccess> _logger = logger;

    public async Task<int> AddProductAsync(Product product)
    {
        try
        {
            using var conn = _factory.Create();

            return await conn.ExecuteScalarAsync<int>(
                "sp_AddProduct",
                new
                {
                    product.ProductName,
                    product.Category,
                    product.Description,
                    product.Price
                },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add product");
            throw;
        }
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            using var conn = _factory.Create();

            return await conn.QueryFirstOrDefaultAsync<Product>(
                "sp_GetProductById",
                new { ProductID = id },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product {Id}", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync()
    {
        try
        {
            using var conn = _factory.Create();

            var result = await conn.QueryAsync<Product>(
                "sp_GetProducts",
                commandType: CommandType.StoredProcedure
            );

            return result.AsList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get products");
            throw;
        }
    }
}
