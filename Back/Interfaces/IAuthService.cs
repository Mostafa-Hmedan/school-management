using Back.Entities;
 
using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<AuthResponse> GetCurrentUserAsync(string userId);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
}
