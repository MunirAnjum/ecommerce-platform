namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid userId,
        string token,
        DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    public bool IsRevoked()
    {
        return RevokedAt.HasValue;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsActive()
    {
        return !IsExpired() && !IsRevoked();
    }
}