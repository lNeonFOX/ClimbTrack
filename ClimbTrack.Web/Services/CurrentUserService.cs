using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClimbTrack.Web.Services;

public interface ICurrentUserService
{
    /// <summary>Id of the signed-in user, or null when anonymous.</summary>
    Task<string?> GetUserIdAsync();
}

public class CurrentUserService(AuthenticationStateProvider provider) : ICurrentUserService
{
    public async Task<string?> GetUserIdAsync()
    {
        var state = await provider.GetAuthenticationStateAsync();
        return state.User.Identity?.IsAuthenticated == true ? state.User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
    }
}
