using System;
using System.ComponentModel.DataAnnotations;

namespace CacheImplementation.Model;

public class Product
{   
    public int ProductID { get; set; }
    [Required(ErrorMessage = "Product Name is required")]
    [MaxLength(100, ErrorMessage = "Product Name cannot exceed 100 characters")]
    public string? ProductName { get; set; }
    [Required(ErrorMessage = "Category is required")]
    [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string? Category { get; set; }
    [Required(ErrorMessage = "Description is required")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }
    public DateTime CreatedOn { get; set; }
}
