using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppRestaurante.BL.Services;
using WebAppRestaurante.Models.Entities.Products;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.ApiService.Controllers
{
    [Authorize(Roles = "Admin , User")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetProducts()
        {
            var products = await productService.GetAllProductsAsync();
            return Ok(new BaseResponseModel { Success = true, Data = products });
        }

        [HttpPost]
        public async Task<ActionResult<ProductModel>> CreateProduct(ProductModel product)
        {
            await productService.CreateProductAsync(product);
            // Create product
            return Ok(new BaseResponseModel { Success = true });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetProductAsync(int id)
        {
            var productModel = await productService.GetProductsAsync(id);
            if (productModel == null)
            {
                return Ok(new BaseResponseModel { Success = false, ErrorMessage = "Not Found" });
            }

            return Ok(new BaseResponseModel { Success = true, Data = productModel });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductModel productModel)
        {
            if (id != productModel.ID || !await productService.ProductModelExists(id))
            {
                return Ok(new BaseResponseModel { Success = false, ErrorMessage = "Bad request" });
            }
            await productService.UpdateProduct(productModel);
            return Ok(new BaseResponseModel { Success = true });

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id) {
            if (!await productService.ProductModelExists(id)) { 
                return Ok(new BaseResponseModel { Success = false, ErrorMessage = "Not Found" } );
            }
            await productService.DeleteProductAsync(id);
            return Ok(new BaseResponseModel {Success = true});
            
        }
    }


}
