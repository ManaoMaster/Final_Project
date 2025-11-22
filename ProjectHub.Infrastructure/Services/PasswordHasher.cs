using ProjectHub.Application.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt; 

namespace ProjectHub.Infrastructure.Services
{
    
    public class PasswordHasher : IPasswordHasher
    {
        
        public string Hash(string password)
        {
            
            return BCryptNet.HashPassword(password, 12);
        }

        
        public bool Verify(string passwordHash, string password)
        {
            
            
            return BCryptNet.Verify(password, passwordHash);
        }
    }
}