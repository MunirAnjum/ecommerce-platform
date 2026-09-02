using AuthService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }

        public string FirstName { get; private set; } = string.Empty;

        public string LastName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public UserRole Role { get; private set; }
        private User()
        {

        }

        public User(
            string firstName,
            string lastName,
            string email,
            string passwordHash
            )
        {
            Id = Guid.NewGuid();

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;

            Role = UserRole.Customer; ;

            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void MakeAdmin()
        {
            Role = UserRole.Admin;
            UpdatedAt = DateTime.UtcNow;
        }
        public ICollection<RefreshToken> RefreshTokens { get; private set; }
            = new List<RefreshToken>();
    }
}
