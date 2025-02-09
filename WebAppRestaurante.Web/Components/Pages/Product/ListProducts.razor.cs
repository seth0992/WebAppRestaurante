using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using WebAppRestaurante.Models.Entities.Products;
using WebAppRestaurante.Models.Models;
using WebAppRestaurante.Web.Components.BaseComponent;

namespace WebAppRestaurante.Web.Components.Pages.Product
{
    public partial class ListProducts
    {

        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        public List<ProductModel> ProductoModels { get; set; } = new();
        
        public required AppModal Modal { get; set; }
        public int DeleteID { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadData();
        }

        protected async Task LoadData() {
            var res = await apiClient.GetFromJsonAsync<BaseResponseModel>("/api/Product");

            if (res != null && res.Success && res.Data != null)
            {
                var dataString = res.Data.ToString();
                if (!string.IsNullOrEmpty(dataString))
                {
                    ProductoModels = JsonConvert.DeserializeObject<List<ProductModel>>(dataString) ?? new List<ProductModel>();
                }
            }
        }

        protected async Task HandleDelete() {
            var res = await apiClient.DeleteAsync<BaseResponseModel>($"/api/Product/{DeleteID}");
            if (res != null && res.Success) {
                toastService.ShowSuccess("Delete product successfully");
                await LoadData();
                Modal.Close();
            }
        }
    }
}
