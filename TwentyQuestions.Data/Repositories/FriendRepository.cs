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
    public interface IFriendRepository : IRepository<FriendEntity, FriendRequest>
    {
    }

    public class FriendRepository : BaseRepository<FriendEntity, FriendRequest>, IFriendRepository
    {
        public FriendRepository(SqlConnection connection, IRepositoryContext context) : base(connection, context)
        {
        }

        public override Task<EntityResponse<FriendEntity>> Get(FriendRequest request)
        {
            if (this.Context.UserId == null)
                throw new InvalidOperationException("RepositoryContext.UserId must be populated");

            request.UserId = this.Context.UserId.Value;

            return base.Get(request);
        }

        protected override void AddGetParameters(SqlParameterCollection sqlParameters, FriendRequest request)
        {
            sqlParameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = request.UserId;
            sqlParameters.Add("@FriendId", SqlDbType.UniqueIdentifier).Value = request.FriendId;
        }

        protected override void AddInsertParameters(SqlParameterCollection sqlParameters, FriendEntity entity)
        {
            throw new NotImplementedException();
        }

        protected override void AddUpdateParameters(SqlParameterCollection sqlParameters, FriendEntity entity)
        {
            throw new NotImplementedException();
        }

        protected override void PopulateEntity(FriendEntity entity, DataRow dataRow, DataSet dataSet)
        {
            entity.FriendId = dataRow.Field<Guid>("FriendId");
            entity.Username = dataRow.Field<string>("Username");
            entity.AvatarFileName = dataRow.Field<string>("AvatarFileName");
        }
    }
}
