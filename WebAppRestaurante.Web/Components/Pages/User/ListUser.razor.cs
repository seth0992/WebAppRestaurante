using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using WebAppRestaurante.Models.Entities.Users;
using Newtonsoft.Json;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.Web.Components.Pages.User
{
    public partial class ListUser
    {

        private List<UserModel> users;

        [CascadingParameter]
        private Task<AuthenticationState> AuthState { get; set; }

        private bool IsCurrentUserSuperAdmin =>
            AuthState.Result.User.IsInRole("SuperAdmin");


        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            //await LoadRoles();
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

    }
}
