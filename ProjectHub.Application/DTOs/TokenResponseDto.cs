namespace ProjectHub.Application.Dtos
{
    public class TokenResponseDto  
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
