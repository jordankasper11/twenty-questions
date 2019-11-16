using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Responses
{
    public class LoginResponse: BaseResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
