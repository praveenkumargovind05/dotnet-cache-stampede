using System;
using CacheImplementation.Model;

namespace CacheImplementation.Repository;
public interface IDataAccess
{
    Task<int> AddProductAsync(Product product);
    Task<Product?> GetProductByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetProductsAsync();
}
