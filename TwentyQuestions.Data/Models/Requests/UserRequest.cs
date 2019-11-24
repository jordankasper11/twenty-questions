using System;
using System.Collections.Generic;
using System.Text;
using TwentyQuestions.Data.Models.Entities;

namespace TwentyQuestions.Data.Models.Requests
{
    public class UserRequest : BaseRequest<UserEntity>
    {
        public string Username { get; set; }

        public bool? FriendSearch { get; set; }
    }
}
