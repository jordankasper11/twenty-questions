using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyQuestions.Data.Models.Requests
{
    public class RefreshTokenRequest : BaseRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
