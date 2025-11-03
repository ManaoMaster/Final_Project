namespace ProjectHub.API.Contracts.Users
{
    public record EditProfileRequest(string Email, string Username, string? ProfilePictureUrl);
}
