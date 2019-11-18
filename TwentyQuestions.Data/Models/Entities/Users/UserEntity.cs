using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class UserEntity : BaseTrackedEntity
    {
        [MaxLength(32)]
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
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

        internal string PasswordSalt { get; set; }

        internal string PasswordHash { get; set; }
    }
}
