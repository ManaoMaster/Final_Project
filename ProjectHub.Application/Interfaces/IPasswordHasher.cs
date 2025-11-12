namespace ProjectHub.Application.Interfaces
{
    // นี่คือ "สัญญา" สำหรับบริการจัดการรหัสผ่าน
    public interface IPasswordHasher
    {
        // สัญญาข้อที่ 1: "สร้าง" Hash จากรหัสผ่าน Text ธรรมดา
        string Hash(string password);

        // สัญญาข้อที่ 2: "ตรวจสอบ" รหัสผ่าน Text กับ Hash ที่เก็บไว้
        // คืนค่า true = รหัสผ่านถูกต้อง
        // คืนค่า false = รหัสผ่านผิด
        bool Verify(string passwordHash, string password);
    }
}