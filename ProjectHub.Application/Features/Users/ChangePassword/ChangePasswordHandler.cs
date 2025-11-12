using MediatR;
using ProjectHub.Application.Interfaces; // (สำหรับ IPasswordHasher)
using ProjectHub.Application.Repositories; // (สำหรับ IUserRepository)
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.Application.Features.Users.ChangePassword
{
    // นี่คือ "ตัวจัดการ" ที่จับคู่กับ ChangePasswordCommand
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;

        // 1. Inject Repository และ Hasher ที่เราสร้างไว้
        public ChangePasswordHandler(IUserRepository userRepository, IPasswordHasher hasher)
        {
            _userRepository = userRepository;
            _hasher = hasher;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // 2. ดึง User จาก DB
            // (เราใช้ GetByIdAsync ที่คุณมีอยู่แล้วใน IUserRepository)
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found."); // (หรือ Exception อื่นๆ ตามที่คุณออกแบบ)
            }

            // 3. ตรวจสอบรหัสผ่านเก่า (CurrentPassword)
            // (เราใช้ Hasher ที่เราเพิ่งสร้าง)
            var isPasswordCorrect = _hasher.Verify(user.Password, request.CurrentPassword);

            // 4. ถ้าไม่ตรง -> โยน Error (จะกลายเป็น 400 Bad Request)
            if (!isPasswordCorrect)
            {
                throw new ArgumentException("Current password is incorrect.");
            }

            // 5. ถ้าตรง -> แฮชรหัสใหม่
            var newPasswordHash = _hasher.Hash(request.NewPassword);

            // 6. อัปเดต Hash ใหม่ลงใน Entity
            user.Password = newPasswordHash;

            // 7. สั่งให้ Repository บันทึกการเปลี่ยนแปลง
            // (เราใช้ UpdateUserAsync ที่คุณมีอยู่แล้วใน IUserRepository)
            await _userRepository.UpdateUserAsync(user);

            // 8. คืนค่า Unit.Value (สำเร็จ)
            return Unit.Value;
        }
    }
}