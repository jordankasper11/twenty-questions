using System;
using System.Collections.Generic;
using System.Text;
using TwentyQuestions.Data.Models.Entities;

namespace TwentyQuestions.Data.Models.Requests
{
    public class GameRequest : BaseRequest<GameEntity>
    {
        public Guid UserId { get; set; }

        public bool? Completed { get; set; }
    }

    public class AskQuestionRequest : BaseRequest
    {
        public Guid GameId { get; set; }

        public string Question { get; set; }
    }

    public class AnswerQuestionRequest : BaseRequest
    {
        public Guid GameId { get; set; }

        public int QuestionId { get; set; }

        public QuestionResponse Response { get; set; }

        public string ResponseExplanation { get; set; }
    }
}
