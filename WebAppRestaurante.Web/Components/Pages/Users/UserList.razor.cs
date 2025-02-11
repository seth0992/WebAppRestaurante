using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using WebAppRestaurante.Models.Entities.Users;
using WebAppRestaurante.Models.Models.User;
using WebAppRestaurante.Models.Models;
using WebAppRestaurante.Web.Components.BaseComponent;

namespace WebAppRestaurante.Web.Components.Pages.Users
{
    // WebAppRestaurante.Web/Components/Pages/Users/UserList.razor.cs
    public partial class UserList
    {
        private List<UserModel> users;
        private UserModel editingUser;
        private UserModel deletingUser;
        private UserViewModel userModel = new();
        private List<RoleViewModel> availableRoles = new();
        private AppModal createEditModal;
        private AppModal deleteModal;

        [CascadingParameter]
        private Task<AuthenticationState> AuthState { get; set; }

        private bool IsCurrentUserSuperAdmin =>
            AuthState.Result.User.IsInRole("SuperAdmin");

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            await LoadRoles();
        }

        private async Task LoadUsers()
        {
            var response = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/UserManagement");
            if (response?.Success == true)
            {
                users = JsonConvert.DeserializeObject<List<UserModel>>(response.Data.ToString());
            }
            else
            {
                ToastService.ShowError("Error al cargar usuarios");
            }
        }

        private async Task LoadRoles()
        {
            // Aquí deberías cargar los roles desde tu API
            availableRoles = new List<RoleViewModel>
        {
            new() { RoleId = 1, RoleName = "Admin" },
            new() { RoleId = 2, RoleName = "User" }
        };
        }

        private void ShowCreateUserModal()
        {
            editingUser = null;
            userModel = new UserViewModel();
            createEditModal.Open();
        }

        private void ShowEditUserModal(UserModel user)
        {
            editingUser = user;
            userModel = new UserViewModel
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            // Marcar los roles que tiene el usuario
            foreach (var role in availableRoles)
            {
                role.IsSelected = user.UserRoles.Any(ur => ur.Role.RoleName == role.RoleName);
            }

            createEditModal.Open();
        }

        private void ShowDeleteConfirmation(UserModel user)
        {
            deletingUser = user;
            deleteModal.Open();
        }

        private async Task HandleUserSubmit()
        {
            try
            {
                BaseResponseModel response;
                if (editingUser == null)
                {
                    var createRequest = new CreateUserRequest
                    {
                        Username = userModel.Username,
                        Password = userModel.Password,
                        FirstName = userModel.FirstName,
                        LastName = userModel.LastName,
                        Email = userModel.Email,
                        RoleIds = availableRoles.Where(r => r.IsSelected).Select(r => r.RoleId).ToList()
                    };

                    response = await ApiClient.PostAsync<BaseResponseModel, CreateUserRequest>(
                        "api/UserManagement", createRequest);
                }
                else
                {
                    var updateRequest = new UpdateUserRequest
                    {
                        FirstName = userModel.FirstName,
                        LastName = userModel.LastName,
                        Email = userModel.Email,
                        RoleIds = availableRoles.Where(r => r.IsSelected).Select(r => r.RoleId).ToList()
                    };

                    response = await ApiClient.PutAsync<BaseResponseModel, UpdateUserRequest>(
                        $"api/UserManagement/{editingUser.ID}", updateRequest);
                }

                if (response.Success)
                {
                    ToastService.ShowSuccess("Usuario guardado exitosamente");
                    await LoadUsers();
                    CloseModal();
                }
                else
                {
                    ToastService.ShowError(response.ErrorMessage ?? "Error al guardar usuario");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Error: {ex.Message}");
            }
        }

        private async Task HandleDeleteUser()
        {
            if (deletingUser != null)
            {
                var response = await ApiClient.DeleteAsync<BaseResponseModel>(
                    $"api/UserManagement/{deletingUser.ID}");

                if (response.Success)
                {
                    ToastService.ShowSuccess("Usuario desactivado exitosamente");
                    await LoadUsers();
                    deleteModal.Close();
                }
                else
                {
                    ToastService.ShowError(response.ErrorMessage ?? "Error al desactivar usuario");
                }
            }
        }

        private void CloseModal()
        {
            createEditModal.Close();
            editingUser = null;
            userModel = new UserViewModel();
        }
    }

    public class UserViewModel
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
    }

    public class RoleViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
