using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;

namespace TwentyQuestions.Data.Repositories
{
    public interface IGameRepository : IRepository<GameEntity, GameRequest>
    {
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

        protected override void AddGetParameters(SqlParameterCollection sqlParameters, GameRequest request)
        {
            sqlParameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = request.UserId;
        }

        protected override void PopulateEntity(GameEntity entity, DataRow dataRow, DataSet dataSet)
        {
            entity.OpponentId = dataRow.Field<Guid>("OpponentId");
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
