using System;
using CacheImplementation.Model;

namespace CacheImplementation.Repository;

public interface IDataAccess
{
    public Task AddProduct(Product product);
    public Product? GetProductByID(int ProductID);
    public List<Product> GetProducts();
}
