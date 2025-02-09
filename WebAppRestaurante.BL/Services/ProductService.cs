using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRestaurante.BL.Repositories;
using WebAppRestaurante.Models.Entities.Products;

namespace WebAppRestaurante.BL.Services
{
    public interface IProductService
    {
        Task<List<ProductModel>> GetAllProductsAsync();
        Task<ProductModel> CreateProductAsync(ProductModel productModel);
        Task<ProductModel> GetProductsAsync(int id);
        Task<bool> ProductModelExists(int id);
        Task UpdateProduct(ProductModel productModel);
        Task DeleteProductAsync(int id); 

    }

    public class ProductService(IProductRepository productRepository) : IProductService
    {
        public Task<ProductModel> CreateProductAsync(ProductModel productModel)
        {
            return productRepository.CreateProductAsync(productModel);
        }

        public Task DeleteProductAsync(int id)
        {
            return productRepository.DeleteProductAsync(id);
        }

        public Task<List<ProductModel>> GetAllProductsAsync()
        {
            return productRepository.GetAllProductsAsync();
        }

        public Task<ProductModel> GetProductsAsync(int id)
        {
            return productRepository.GetProductsAsync(id);
        }

        public Task<bool> ProductModelExists(int id)
        {
            return productRepository.ProductModelExists(id);
        }

        public Task UpdateProduct(ProductModel productModel)
        {
            return productRepository.UpdateProduct(productModel);
        }
    }
}
