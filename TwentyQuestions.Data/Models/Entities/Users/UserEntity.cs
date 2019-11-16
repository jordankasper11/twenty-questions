using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class UserEntity : BaseTrackedEntity
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string AvatarFileExtension { get; set; }

        public string AvatarUrl
        {
            get
            {
                if (this.Id != null && !String.IsNullOrWhiteSpace(this.AvatarFileExtension))
                    return $"/avatars/{this.Id}.{this.AvatarFileExtension}";

                return null;
            }
        }
    }
}
