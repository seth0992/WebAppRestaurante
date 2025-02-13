using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using WebAppRestaurante.Models.Models;
using Newtonsoft.Json;
using System.Net.Http;
using WebAppRestaurante.Web.Authentication;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Blazored.Toast.Services;
using System.Net;

namespace WebAppRestaurante.Web;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IToastService _toastService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private bool _isRefreshing = false;
    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public ApiClient(
        HttpClient httpClient,
        NavigationManager navigationManager,
        ProtectedLocalStorage localStorage,
        IToastService toastService,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        _localStorage = localStorage;
        _toastService = toastService;
        _authStateProvider = authStateProvider;
    }

    private async Task<string> GetTokenAsync()
    {
        var sessionState = (await _localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;
        return sessionState?.Token ?? string.Empty;
    }

    private async Task SetAuthorizationHeader()
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<bool> RefreshTokenAsync()
    {
        try
        {
            await _semaphore.WaitAsync();
            if (_isRefreshing) return true;

            _isRefreshing = true;
            var sessionState = (await _localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;

            if (sessionState == null || string.IsNullOrEmpty(sessionState.RefreshToken))
            {
                return false;
            }

            // Quitar el header de autorizaci�n para la llamada de refresh
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var response = await _httpClient.GetAsync($"api/Auth/loginByRefreshToken?refreshToken={sessionState.RefreshToken}");

            if (response.IsSuccessStatusCode)
            {
                var newSession = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
                if (newSession != null)
                {
                    await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(newSession);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error al refrescar el token: {ex.Message}");
            return false;
        }
        finally
        {
            _isRefreshing = false;
            _semaphore.Release();
        }
    }

    private async Task HandleUnauthorizedResponse()
    {
        var refreshSuccess = await RefreshTokenAsync();
        if (!refreshSuccess)
        {
            await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
            _navigationManager.NavigateTo("/login", true);
        }
    }

    public async Task<T?> PatchAsync<T>(string url)
    {
        var response = await _httpClient.PatchAsync(url, null);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
        return default;
    }
    public async Task<T?> GetFromJsonAsync<T>(string requestUri)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.GetAsync(requestUri);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TResponse, TRequest>(string requestUri, TRequest content)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync(requestUri, content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.PostAsJsonAsync(requestUri, content);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    public async Task<TResponse?> PutAsync<TResponse, TRequest>(string requestUri, TRequest content)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync(requestUri, content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.PutAsJsonAsync(requestUri, content);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string requestUri)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(requestUri);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.DeleteAsync(requestUri);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var error = await response.Content.ReadAsStringAsync();
        _toastService.ShowError($"Error: {response.StatusCode} - {error}");
    }

    private void HandleException(Exception ex)
    {
        _toastService.ShowError($"Error en la solicitud: {ex.Message}");
    }
}
