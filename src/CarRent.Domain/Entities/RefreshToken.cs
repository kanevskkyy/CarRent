namespace CarRent.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime ExpiresOnUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOnUtc { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOnUtc;
    public bool IsActive => !IsRevoked && !IsExpired;
}
