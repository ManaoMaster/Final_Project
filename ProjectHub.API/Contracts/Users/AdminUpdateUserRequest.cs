namespace ProjectHub.API.Contracts.Users
{
    public class AdminUpdateUserRequest
    {

        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        
        public string? Role { get; set; }

        
        public string? ProfilePictureUrl { get; set; }
    }
}
