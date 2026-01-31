using CacheImplementation.Model;
using CacheImplementation.Repository;
using CacheImplementation.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CacheImplementation.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IDataAccess dataAccess, CachedRepository cachedRepository, ILogger<ProductController> logger) : ControllerBase
    {
        private readonly IDataAccess _dataAccess = dataAccess;
        private readonly CachedRepository _cache = cachedRepository;
        private readonly ILogger<ProductController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAllProducts(CancellationToken ct)
        {
            try
            {
                var data = await _cache.GetOrCreateAsync("ALL_PRODUCTS", async () => await _dataAccess.GetProductsAsync(),
                new()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                }, ct);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all products");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching products");
            }
        }

        [HttpGet("{productID:int}")]
        public async Task<IActionResult> GetProductByID(int productID, CancellationToken ct)
        {
            try
            {
                if (productID <= 0)
                    return BadRequest("ProductID must be greater than 0");

                var data = await _cache.GetOrCreateAsync($"PRODUCT_{productID}", async () => await _dataAccess.GetProductByIdAsync(productID),
                new()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                }, ct);

                if (data == null)
                    return NotFound($"Product with ID {productID} not found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product with ID: {ProductID}", productID);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching product");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product, CancellationToken ct)
        {
            try
            {
                if (product == null)
                    return BadRequest("Product cannot be null");

                if (string.IsNullOrWhiteSpace(product.ProductName))
                    return BadRequest("ProductName is required");

                // Invalidate cache before adding
                await _cache.RemoveAsync("ALL_PRODUCTS", ct);
                
                var productID = await _dataAccess.AddProductAsync(product);
                return CreatedAtAction(nameof(GetProductByID), new { productID = productID}, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding product");
            }
        }
    }
}