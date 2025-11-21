namespace ProjectHub.API.Contracts.Users
{
    public class ResetPasswordRequest
    {
        public string Token { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
