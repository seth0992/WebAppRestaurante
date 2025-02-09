using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using WebAppRestaurante.Models.Entities.Products;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.Web.Components.Pages.Product
{
    public partial class CreateProduct
    {
        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        [Inject]
        public required NavigationManager navigationManager { get; set; }
        public ProductModel Model { get; set; } = new();

        public async Task Submit() {
            var res = await apiClient.PostAsync<BaseResponseModel, ProductModel>("/api/Product", Model);
            if (res != null && res.Success) { 
            
                toastService.ShowSuccess("Product created successfully");
                navigationManager.NavigateTo("/product");
            }
        
        }
    }
}
