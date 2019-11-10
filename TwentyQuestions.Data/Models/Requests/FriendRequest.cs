using System;
using System.Collections.Generic;
using System.Text;
using TwentyQuestions.Data.Models.Entities;

namespace TwentyQuestions.Data.Models.Requests
{
    public class FriendRequest : BaseRequest<FriendEntity>
    {
        public Guid UserId { get; set; }
    }
}
