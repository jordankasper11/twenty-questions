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

        public string AvatarFileExtension { get; set; }

        public string AvatarUrl
        {
            get
            {
                if (this.Id != null && !String.IsNullOrWhiteSpace(this.AvatarFileExtension))
                    return $"/avatars/{this.FriendId}.{this.AvatarFileExtension}";

                return null;
            }
        }
    }
}
