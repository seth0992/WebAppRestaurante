using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using WebAppRestaurante.Models.Entities.Users;
using WebAppRestaurante.Models.Models.User;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.Web.Components.Pages.User
{
    public partial class UpdateUser
    {
        [Inject]
        public required ApiClient apiClient { get; set; }
        [Inject]
        public required IToastService toastService { get; set; }
        [Inject]
        public required NavigationManager navigationManager { get; set; }
        [Parameter]        
        public int Id { get; set; }

        private UpdateUserFormModel model = new();
        private bool isLoaded;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadRoles();
                await LoadUser();
                isLoaded = true;
            }
            catch (Exception)
            {
                toastService.ShowError("Error al cargar la información del usuario");
                navigationManager.NavigateTo("/users");
            }
        }

        private async Task LoadRoles()
        {
            var response = await apiClient.GetFromJsonAsync<BaseResponseModel>("api/Role");
            if (response?.Success == true && response.Data != null)
            {
                var roles = JsonConvert.DeserializeObject<List<RoleModel>>(response.Data.ToString()) ?? new();
                model.RoleSelections = roles.Select(r => new RoleSelectionModel
                {
                    RoleId = r.ID,
                    RoleName = r.RoleName,
                    IsSelected = false
                }).ToList();
            }
        }

        private async Task LoadUser()
        {
            var response = await apiClient.GetFromJsonAsync<BaseResponseModel>($"api/UserManagement/{Id}");
            if (response?.Success == true && response.Data != null)
            {
                var user = JsonConvert.DeserializeObject<UserModel>(response.Data.ToString());
                if (user != null)
                {
                    model.UserData = new UpdateUserDTO
                    {
                        ID = user.ID,
                        Username = user.Username, // Aseguramos que se cargue el username
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        IsActive = user.IsActive
                    };

                    // Marcar los roles que el usuario ya tiene
                    foreach (var userRole in user.UserRoles)
                    {
                        var roleSelection = model.RoleSelections
                            .FirstOrDefault(r => r.RoleId == userRole.RoleID);
                        if (roleSelection != null)
                        {
                            roleSelection.IsSelected = true;
                        }
                    }
                }
            }
            else
            {
                toastService.ShowError("Error al cargar la información del usuario");
                navigationManager.NavigateTo("/users");
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                if (!model.RoleSelections.Any(r => r.IsSelected))
                {
                    toastService.ShowWarning("Debe seleccionar al menos un rol");
                    return;
                }

                var updateUserDto = new UpdateUserDTO
                {
                    ID = Id,
                    Username = model.UserData.Username, // Añadimos el username
                    FirstName = model.UserData.FirstName,
                    LastName = model.UserData.LastName,
                    Email = model.UserData.Email,
                    IsActive = model.UserData.IsActive,
                    RoleIds = model.RoleSelections
                        .Where(r => r.IsSelected)
                        .Select(r => r.RoleId)
                        .ToList()
                };

                var response = await apiClient.PutAsync<BaseResponseModel, UpdateUserDTO>(
                    $"api/UserManagement/{Id}", updateUserDto);

                if (response?.Success == true)
                {
                    toastService.ShowSuccess("Usuario actualizado exitosamente");
                    navigationManager.NavigateTo("/users");
                }
                else
                {
                    toastService.ShowError(response?.ErrorMessage ?? "Error al actualizar usuario");
                }
            }
            catch (Exception)
            {
                toastService.ShowError("Error al actualizar usuario");
            }
        }

        private class UpdateUserFormModel
        {
            public UpdateUserDTO UserData { get; set; } = new();
            public List<RoleSelectionModel> RoleSelections { get; set; } = new();
        }

        private class RoleSelectionModel
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public bool IsSelected { get; set; }
        }
    }
}
