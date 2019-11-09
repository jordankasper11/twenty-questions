using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class GameEntity : BaseTrackedEntity
    {
        public string Subject { get; set; }

        public Guid OpponentId { get; set; }

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
