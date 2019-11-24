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
        Task AcceptInvitation(Guid gameId);

        Task DeclineInvitation(Guid gameId);
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

        public override Task<Guid> Insert(FriendEntity entity)
        {
            entity.Status = EntityStatus.Pending;

            return base.Insert(entity);
        }

        public override async Task Delete(Guid id)
        {
            var friend = await Get(id);

            if (friend != null)
            {
                friend.Status = EntityStatus.Deleted;

                await Update(friend);
            }
        }

        public async Task AcceptInvitation(Guid gameId)
        {
            await UpdateStatus(gameId, EntityStatus.Active);
        }

        public async Task DeclineInvitation(Guid gameId)
        {
            await UpdateStatus(gameId, EntityStatus.Deleted);
        }

        protected override void AddGetParameters(SqlParameterCollection sqlParameters, FriendRequest request)
        {
            sqlParameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = request.UserId;
            sqlParameters.Add("@FriendId", SqlDbType.UniqueIdentifier).Value = request.FriendId;
        }

        protected override void AddInsertParameters(SqlParameterCollection sqlParameters, FriendEntity entity)
        {
            sqlParameters.Add("@FriendId", SqlDbType.UniqueIdentifier).Value = entity.FriendId;
        }

        protected override void AddUpdateParameters(SqlParameterCollection sqlParameters, FriendEntity entity)
        {
        }

        protected override void PopulateEntity(FriendEntity entity, DataRow dataRow, DataSet dataSet)
        {
            entity.FriendId = dataRow.Field<Guid>("FriendId");
            entity.Username = dataRow.Field<string>("Username");
            entity.AvatarFileName = dataRow.Field<string>("AvatarFileName");
        }

        private async Task UpdateStatus(Guid id, EntityStatus status)
        {
            var friend = await Get(id);

            if (friend == null)
                throw new InvalidOperationException("Invalid Id");

            friend.Status = status;

            await Update(friend);
        }
    }
}
