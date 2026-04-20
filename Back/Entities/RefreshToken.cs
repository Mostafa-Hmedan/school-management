namespace Back.Entities;
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public AppUser AppUser { get; set; } = null!;
}
