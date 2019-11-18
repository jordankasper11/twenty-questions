using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyQuestions.Data.Models.Requests
{
    public class RegistrationRequest : BaseRequest
    {
        [MaxLength(32)]
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6)]
        [Required]
        public string Password { get; set; }
    }
}
