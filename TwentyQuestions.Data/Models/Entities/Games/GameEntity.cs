﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace TwentyQuestions.Data.Models.Entities
{
    public class GameEntity : BaseTrackedEntity
    {
        public string Subject { get; set; }

        public Guid OpponentId { get; set; }

        public Guid? FriendId { get; set; }

        public string FriendUsername { get; set; }

        [JsonIgnore]
        public string FriendAvatarFileExtension { get; set; }

        public string FriendAvatarUrl
        {
            get
            {
                if (this.Id != null && this.FriendId != null && !String.IsNullOrWhiteSpace(this.FriendAvatarFileExtension))
                    return $"/avatars/{this.FriendId.Value}.{this.FriendAvatarFileExtension}";

                return null;
            }
        }

        public int MaxQuestions { get; set; }

        public bool Completed
        {
            get
            {
                return this.Questions?.Any(q => q.Response == QuestionResponse.Correct || q.Response == QuestionResponse.GameOver) == true || this.Questions?.Count >= this.MaxQuestions;
            }
        }

        public List<QuestionEntity> Questions { get; set; }
    }
}
