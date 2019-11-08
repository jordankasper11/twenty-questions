using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public enum QuestionResponse
    {
        Yes,
        No,
        Sometimes,
        Probably,
        ProbablyNot,
        Correct,
        GameOver
    }

    public class QuestionEntity
    {
        public Guid Id { get; set; }

        public string Question { get; set; }

        public QuestionResponse Response { get; set; }

        public string ResponseExplanation { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
