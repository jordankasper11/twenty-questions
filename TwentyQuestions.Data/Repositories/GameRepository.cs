using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;

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

        protected override void AddGetParameters(SqlParameterCollection sqlParameters, GameRequest request)
        {
        }

        protected override void PopulateEntity(GameEntity entity, DataRow dataRow, DataSet dataSet)
        {
            entity.OpponentId = dataRow.Field<Guid>("OpponentId");
            entity.MaxQuestions = dataRow.Field<int>("MaxQuestions");
        }

        protected override void AddInsertParameters(SqlParameterCollection sqlParameters, GameEntity entity)
        {
            throw new NotImplementedException();
        }

        protected override void AddUpdateParameters(SqlParameterCollection sqlParameters, GameEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
