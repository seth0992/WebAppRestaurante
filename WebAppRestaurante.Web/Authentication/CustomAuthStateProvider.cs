using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAppRestaurante.Web.Authentication
{
    public class CustomAuthStateProvider(ProtectedLocalStorage localStorage) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            throw new NotImplementedException();
        }

        public async Task MarkUserAsAuthenticated(string token) {

            await localStorage.SetAsync("authToken", token);
            var identity = GetClaimsIdentity(token);
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            
        }

        private ClaimsIdentity GetClaimsIdentity(string token)
        {
           var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            return new ClaimsIdentity(claims, "jwt");
        }
    }
}
