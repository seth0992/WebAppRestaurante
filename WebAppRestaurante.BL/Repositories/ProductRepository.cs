using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRestaurante.Database.Data;
using WebAppRestaurante.Models.Entities.Products;

namespace WebAppRestaurante.BL.Repositories
{

    public interface IProductRepository
    {
        Task<List<ProductModel>> GetAllProductsAsync();
        Task<ProductModel> CreateProductAsync(ProductModel productModel);
        Task<ProductModel> GetProductsAsync(int id);
        Task<bool> ProductModelExists(int id);
        Task UpdateProduct(ProductModel productModel);
        Task DeleteProductAsync(int id);
    }

    public class ProductRepository(AppDbContext appDbContext) : IProductRepository
    {
        public async Task<ProductModel> CreateProductAsync(ProductModel productModel)
        {
             appDbContext.Products.Add(productModel);
            await appDbContext.SaveChangesAsync();
            return productModel;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await appDbContext.Products.FirstOrDefaultAsync(p => p.ID == id);
            appDbContext.Products.Remove(product);
            await appDbContext.SaveChangesAsync();
        }

        public async Task<List<ProductModel>> GetAllProductsAsync()
        {
            return await appDbContext.Products.ToListAsync();
        }

        public Task<ProductModel> GetProductsAsync(int id)
        {
            return appDbContext.Products.FirstOrDefaultAsync(p => p.ID == id);
        }

        public Task<bool> ProductModelExists(int id)
        {
            return appDbContext.Products.AnyAsync(p => p.ID == id); 
        }

        public async Task UpdateProduct(ProductModel productModel)
        {
            appDbContext.Entry(productModel).State = EntityState.Modified;
            await appDbContext.SaveChangesAsync();
        }
    }
}
