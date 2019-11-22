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

        public string AvatarFileName { get; set; }

        public string AvatarUrl
        {
            get
            {
                if (this.Id != null && !String.IsNullOrWhiteSpace(this.AvatarFileName))
                    return $"/avatars/{this.FriendId}/{this.AvatarFileName}";

                return null;
            }
        }
    }
}
