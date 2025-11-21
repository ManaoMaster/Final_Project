namespace ProjectHub.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }

        public Users User { get; set; } = default!;
    }
}
