using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using WebAppRestaurante.Models.Models;
using Newtonsoft.Json;
using System.Net.Http;
using WebAppRestaurante.Web.Authentication;
using Newtonsoft.Json.Linq;

namespace WebAppRestaurante.Web;

public class ApiClient(HttpClient httpClient,
    ProtectedLocalStorage localStorage,
    NavigationManager navigationManager,
    AuthenticationStateProvider authStateProvider)
{

    public async Task SetAuthorizationHeader()
    {
        try
        {
            var sessionState = (await localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;
            
            if (sessionState != null && !string.IsNullOrEmpty(sessionState.Token))
            {

                if (sessionState.TokenExpired < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    await ((CustomAuthStateProvider)authStateProvider).MarkUserAsLoggedOut();
                }
                else if (sessionState.TokenExpired < DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds())
                {
                    var res = await httpClient.GetFromJsonAsync<LoginResponseModel>($"/api/Auth/loginByRefreshToken?refreshToken= {sessionState.RefreshToken}");
                    if (res != null)
                    {
                        await ((CustomAuthStateProvider)authStateProvider).MarkUserAsAuthenticated(res);
                        // Agregar el token a los headers
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", res.Token);
                    }
                }
                else {
                    // Agregar el token a los headers
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", sessionState.Token);
                }
               
            }

           
        }
        catch (Exception)
        {
            await ((CustomAuthStateProvider)authStateProvider).MarkUserAsLoggedOut();
            navigationManager.NavigateTo("/login");
        }
    }

    //private async Task<LoginResponseModel?> RefreshToken(string refreshToken)
    //{
    //    return await httpClient.GetFromJsonAsync<LoginResponseModel>(
    //        $"/api/auth/refreshToken?token={refreshToken}");
    //}

    public async Task<T> GetFromJsonAsync<T>(string path)
    {
        await SetAuthorizationHeader();
        var result = await httpClient.GetFromJsonAsync<T>(path);
        return result ?? throw new InvalidOperationException("Received null response from the server.");
    }

    public async Task<T?> PatchAsync<T>(string requestUri, object? content = null)
    {
        var jsonContent = content != null ? JsonContent.Create(content) : null;
        var response = await httpClient.PatchAsync(requestUri, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ApplicationException($"Error en la solicitud: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result;
    }
        
    public async Task<T1?> PostAsync<T1, T2>(string path, T2 postModel)
    {
        await SetAuthorizationHeader();
        try
        {
            var response = await httpClient.PostAsJsonAsync(path, postModel);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T1>();
        }
        catch (Exception ex)
        {
            //logger.LogError(ex, "Error making POST request to {Path}", path);
            throw new HttpRequestException($"Error in request to {path}", ex);
        }
    }

    public async Task<T1> PutAsync<T1, T2>(string path, T2 postModel)
    {
        await SetAuthorizationHeader();
        var response = await httpClient.PutAsJsonAsync(path, postModel);
        if (response != null && response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<T1>(await response.Content.ReadAsStringAsync()!)!;
        }
        return default!;
    }

    public async Task<T> DeleteAsync<T>(string path)
    {
        await SetAuthorizationHeader();
        var result = await httpClient.DeleteFromJsonAsync<T>(path);
        return result ?? throw new InvalidOperationException("Received null response from the server.");
    }
}


