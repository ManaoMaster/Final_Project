namespace ProjectHub.API.Contracts.Users
{
    // นี่คือ DTO ที่ Swagger/API จะใช้รับข้อมูล
    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}   