using System;
using CacheImplementation.Model;

namespace CacheImplementation.Repository;

public class DataAccess : IDataAccess
{
    private List<Product> Products { get; set; } = [];
    public async Task AddProduct(Product product)
    {
        try
        {
            await Task.Delay(100);
            Products.Add(product);
        }
        catch
        {
            throw;
        }
    }

    public Product? GetProductByID(int ProductID)
    {
        try
        {
            return Products.FirstOrDefault(product => product.ProductID == ProductID);
        }
        catch
        {
            throw;
        }
    }

    public List<Product> GetProducts()
    {
        try
        {
            return Products;
        }
        catch
        {
            throw;
        }
    }
}
