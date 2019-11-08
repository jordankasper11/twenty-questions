using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class UserEntity : BaseTrackedEntity
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }
    }
}
