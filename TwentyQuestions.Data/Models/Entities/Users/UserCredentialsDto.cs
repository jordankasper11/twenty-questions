using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class UserCredentialsDto
    {
        public Guid? UserId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string PasswordSalt { get; set; }

        public string PasswordHash { get; set; }

        public string RefreshToken { get; set; }
    }
}
