using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;

namespace TwentyQuestions.Data.Repositories
{
    public interface INotificationsRepository : IRepository
    {
        Task<NotificationsEntity> Get(DateTime? lastChecked);
    }

    public class NotificationsRepository : BaseRepository, INotificationsRepository
    {
        public NotificationsRepository(SqlConnection connection, IRepositoryContext context) : base(connection, context)
        {
        }

        public async Task<NotificationsEntity> Get(DateTime? gamesLastChecked)
        {
            if (this.Context.UserId == null)
                throw new UnauthorizedAccessException();

            await EnsureConnectionOpen();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "Notifications_Get";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = this.Context.UserId.Value });
                sqlCommand.Parameters.Add(new SqlParameter("@GamesLastChecked", SqlDbType.DateTime) { Value = gamesLastChecked });

                using (var sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();

                    dataTable.Load(sqlDataReader);

                    if (dataTable.Rows.Count > 0)
                    {
                        var notfications = new NotificationsEntity();

                        notfications.FriendNotifications = dataTable.Rows[0].Field<bool>("FriendNotifications");
                        notfications.GameNotifications = dataTable.Rows[0].Field<bool>("GameNotifications");                       

                        return notfications;
                    }
                }
            }

            return null;
        }
    }
}

