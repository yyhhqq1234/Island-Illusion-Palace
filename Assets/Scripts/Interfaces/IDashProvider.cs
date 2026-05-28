public interface IDashProvider
{
    bool isDashing { get; }
    float dashSpeed { get; }
    float dashDuration { get; }
    float dashCooldown { get; }
    void StartDash();
    void SetInvulnerable(bool invulnerable);
}
