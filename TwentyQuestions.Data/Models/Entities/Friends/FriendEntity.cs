using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class FriendEntity : BaseTrackedEntity
    {
        public Guid FriendId { get; set; }

        public string Username { get; set; }
    }
}
