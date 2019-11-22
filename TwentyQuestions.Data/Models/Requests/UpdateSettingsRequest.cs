using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyQuestions.Data.Models.Requests
{
    public class UpdateSettingsRequest : BaseRequest
    {
        [Required]
        public Guid? UserId { get; set; }

        [MaxLength(32)]
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [MinLength(6)]
        [Required]
        public string NewPassword { get; set; }
    }
}
