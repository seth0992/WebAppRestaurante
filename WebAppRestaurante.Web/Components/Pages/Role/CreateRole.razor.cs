using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using WebAppRestaurante.Models.Entities.Users;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.Web.Components.Pages.Role
{
    public partial class CreateRole
    {
        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        [Inject]
        public required NavigationManager navigationManager { get; set; }

        private RoleModel role = new();

        private async Task HandleSubmit()
        {
            try
            {
                var response = await apiClient.PostAsync<BaseResponseModel, RoleModel>("api/Role", role);
                if (response?.Success == true)
                {
                    toastService.ShowSuccess("Rol creado exitosamente");
                    navigationManager.NavigateTo("/roles");
                }
                else
                {
                    toastService.ShowError(response?.ErrorMessage ?? "Error al crear rol");
                }
            }
            catch (Exception)
            {
                toastService.ShowError("Error al crear rol");
            }
        }
    }
}
