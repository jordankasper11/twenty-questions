using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;

namespace TwentyQuestions.Data.Repositories
{
    public interface IGameRepository : IRepository<GameEntity, GameRequest>
    {
        Task AcceptInvitation(Guid gameId);

        Task DeclineInvitation(Guid gameId);

        Task AskQuestion(AskQuestionRequest request);

        Task AnswerQuestion(AnswerQuestionRequest request);
    }

    public class GameRepository : BaseRepository<GameEntity, GameRequest>, IGameRepository
    {
        public GameRepository(SqlConnection connection, IRepositoryContext context) : base(connection, context)
        {
        }

        public override Task<EntityResponse<GameEntity>> Get(GameRequest request)
        {
            if (this.Context.UserId == null)
                throw new InvalidOperationException("RepositoryContext.UserId must be populated");

            request.UserId = this.Context.UserId.Value;

            return base.Get(request);
        }

        public override Task<Guid> Insert(GameEntity entity)
        {
            entity.Status = EntityStatus.Pending;

            return base.Insert(entity);
        }

        public override Task Update(GameEntity entity)
        {
            if (entity.CreatedBy != this.Context.UserId && entity.OpponentId != this.Context.UserId)
                throw new InvalidOperationException("Only participants in this game can make updates");

            return base.Update(entity);
        }

        public async Task AskQuestion(AskQuestionRequest request)
        {
            var game = await Get(request.GameId);

            if (game == null)
                throw new InvalidOperationException("Invalid GameId");

            if (game.OpponentId != this.Context.UserId)
                throw new InvalidOperationException("Only the challenged user can ask a question");

            if (game.ModifiedBy == this.Context.UserId || game.Questions?.LastOrDefault() != null && game.Questions.Last().Response == null)
                throw new InvalidOperationException("Waiting for opponent to respond");

            if (game.Completed)
                throw new InvalidOperationException("The game has alreaddy been completed");

            if (game.Questions == null)
                game.Questions = new List<QuestionEntity>();

            var question = new QuestionEntity();

            question.Id = game.Questions.Count + 1;
            question.Question = request.Question;
            question.CreatedDate = DateTime.UtcNow;

            game.Questions.Add(question);

            await Update(game);
        }

        public async Task AnswerQuestion(AnswerQuestionRequest request)
        {
            var game = await Get(request.GameId);

            if (game == null)
                throw new InvalidOperationException("Invalid GameId");

            if (game.CreatedBy != this.Context.UserId)
                throw new InvalidOperationException("Only the challenger can respond to a question");

            if (game.ModifiedBy == this.Context.UserId || game.Questions?.LastOrDefault().Response != null)
                throw new InvalidOperationException("Waiting for opponent to respond");

            if (game.Completed)
                throw new InvalidOperationException("The game has alreaddy been completed");

            var question = game.Questions?.LastOrDefault();

            if (question == null)
                throw new InvalidOperationException("Invalid QuestionId");

            if (question.Id != request.QuestionId)
                throw new InvalidOperationException("This question has already been answered");

            question.Response = request.Response;
            question.ResponseExplanation = !String.IsNullOrWhiteSpace(request.ResponseExplanation) ? request.ResponseExplanation : null;

            await Update(game);
        }

        public async Task AcceptInvitation(Guid gameId)
        {
            await UpdateStatus(gameId, EntityStatus.Active);
        }

        public async Task DeclineInvitation(Guid gameId)
        {
            await UpdateStatus(gameId, EntityStatus.Deleted);
        }

        private async Task UpdateStatus(Guid gameId, EntityStatus status)
        {
            var game = await Get(gameId);

            if (game == null)
                throw new InvalidOperationException("Invalid GameId");

            if (game.Status != EntityStatus.Pending)
                throw new InvalidOperationException("This game is not in pending status");

            if (game.OpponentId != this.Context.UserId)
                throw new InvalidOperationException("Only the challenged user can accept or decline an invitation");

            game.Status = status;

            await Update(game);
        }

        protected override void AddGetParameters(SqlParameterCollection sqlParameters, GameRequest request)
        {
            sqlParameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = request.UserId;
            sqlParameters.Add("@Completed", SqlDbType.Bit).Value = request.Completed;
        }

        protected override void PopulateEntity(GameEntity entity, DataRow dataRow, DataSet dataSet)
        {
            entity.OpponentId = dataRow.Field<Guid>("OpponentId");
            entity.FriendId = dataRow.Field<Guid>("FriendId");
            entity.FriendUsername = dataRow.Field<string>("FriendUsername");
            entity.FriendAvatarFileName = dataRow.Field<string>("FriendAvatarFileName");
            entity.Subject = dataRow.Field<string>("Subject");
            entity.MaxQuestions = dataRow.Field<int>("MaxQuestions");
            entity.Questions = Deserialize<List<QuestionEntity>>(dataRow.Field<string>("Questions"));
        }

        protected override void AddInsertParameters(SqlParameterCollection sqlParameters, GameEntity entity)
        {
            sqlParameters.Add("@OpponentId", SqlDbType.UniqueIdentifier).Value = entity.OpponentId;
            sqlParameters.Add("@Subject", SqlDbType.NVarChar).Value = entity.Subject;
            sqlParameters.Add("@MaxQuestions", SqlDbType.Int).Value = entity.MaxQuestions;
            sqlParameters.Add("@Completed", SqlDbType.Bit).Value = entity.Completed;
            sqlParameters.Add("@Questions", SqlDbType.NVarChar).Value = Serialize(entity.Questions);
        }

        protected override void AddUpdateParameters(SqlParameterCollection sqlParameters, GameEntity entity)
        {
            sqlParameters.Add("@OpponentId", SqlDbType.UniqueIdentifier).Value = entity.OpponentId;
            sqlParameters.Add("@Subject", SqlDbType.NVarChar).Value = entity.Subject;
            sqlParameters.Add("@MaxQuestions", SqlDbType.Int).Value = entity.MaxQuestions;
            sqlParameters.Add("@Completed", SqlDbType.Bit).Value = entity.Completed;
            sqlParameters.Add("@Questions", SqlDbType.NVarChar).Value = Serialize(entity.Questions);
        }
    }
}