using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Back.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtTokenProvider _jwtTokenProvider;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        JwtTokenProvider jwtTokenProvider,
        AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenProvider = jwtTokenProvider;
        _context = context;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("البريد الإلكتروني أو كلمة المرور غير صحيحة");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
            throw new UnauthorizedAccessException("البريد الإلكتروني أو كلمة المرور غير صحيحة");

        var roles = await _userManager.GetRolesAsync(user);

        var token = _jwtTokenProvider.GenerateJwtToken(new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList(),
            Permissions = []
        });

        // حفظ الـ RefreshToken في DB
        var refreshToken = new RefreshToken
        {
            Token = token.RefreshToken!,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            AppUserId = user.Id
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Email = user.Email!,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Token = token
        };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<AuthResponse> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("المستخدم غير موجود");

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            Email = user.Email!,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenRecord = await _context.RefreshTokens
            .Include(r => r.AppUser)
            .FirstOrDefaultAsync(r => r.Token == refreshToken)
            ?? throw new UnauthorizedAccessException("الـ Refresh Token غير صالح");

        if (tokenRecord.IsRevoked)
            throw new UnauthorizedAccessException("الـ Refresh Token تم إلغاؤه");

        if (tokenRecord.Expires < DateTime.UtcNow)
            throw new UnauthorizedAccessException("انتهت صلاحية الـ Refresh Token");

        var user = tokenRecord.AppUser;
        var roles = await _userManager.GetRolesAsync(user);

        var newToken = _jwtTokenProvider.GenerateJwtToken(new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList(),
            Permissions = []
        });

        // إلغاء القديم وحفظ الجديد
        tokenRecord.IsRevoked = true;

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = newToken.RefreshToken!,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            AppUserId = user.Id
        });

        await _context.SaveChangesAsync();

        return newToken;
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var tokenRecord = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken)
            ?? throw new KeyNotFoundException("الـ Refresh Token غير موجود");

        tokenRecord.IsRevoked = true;
        await _context.SaveChangesAsync();
    }
}
