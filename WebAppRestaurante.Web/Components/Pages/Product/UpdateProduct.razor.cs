using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using WebAppRestaurante.Models.Entities.Products;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.Web.Components.Pages.Product
{
    public partial class UpdateProduct : ComponentBase
    {

        [Parameter]
        public int ID { get; set; }
        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        [Inject]
        public required NavigationManager navigationManager { get; set; }
        public ProductModel Model { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            var res = await apiClient.GetFromJsonAsync<BaseResponseModel>($"/api/Product/{ID}");
            if (res != null && res.Success && res.Data != null)
            {
                var dataString = res.Data.ToString();
                if (!string.IsNullOrEmpty(dataString))
                {
                    Model = JsonConvert.DeserializeObject<ProductModel>(dataString) ?? new ProductModel();
                }
            }
            await base.OnInitializedAsync();
        }

        public async Task Submit() {
            var res = await apiClient.PutAsync<BaseResponseModel, ProductModel>($"/api/Product/{ID}", Model);
            if (res != null && res.Success) {
                toastService.ShowSuccess("Product created successfully");
                navigationManager.NavigateTo("/product");
            }
        }
    }
}
