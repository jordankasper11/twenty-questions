using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class UserCredentials
    {
        public string PasswordSalt { get; set; }

        public string PasswordHash { get; set; }

        public string RefreshToken { get; set; }
    }
}
