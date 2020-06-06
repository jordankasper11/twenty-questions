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
    public interface INotificationRepository : IRepository
    {
        Task<IEnumerable<NotificationEntity>> Get(Guid userId);

        Task<NotificationEntity> Insert(NotificationEntity notification);

        Task<IEnumerable<NotificationEntity>> Delete(NotificationDeleteRequest request);
    }

    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        public NotificationRepository(SqlConnection connection, IRepositoryContext context) : base(connection, context)
        {
        }

        public async Task<IEnumerable<NotificationEntity>> Get(Guid userId)
        {
            await EnsureConnectionOpen();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "Notification_Get";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

                using (var sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();

                    dataTable.Load(sqlDataReader);

                    return dataTable?.AsEnumerable()
                        .Select(dataRow =>
                        {
                            var notfication = new NotificationEntity();

                            notfication.Id = dataRow.Field<Guid>("Id");
                            notfication.UserId = dataRow.Field<Guid>("UserId");
                            notfication.CreatedDate = dataRow.Field<DateTime>("CreatedDate");
                            notfication.Type = dataRow.Field<NotificationType>("Type");
                            notfication.RecordId = dataRow.Field<Guid?>("RecordId");

                            return notfication;
                        })
                        .ToList();
                }
            }
        }

        public async Task<NotificationEntity> Insert(NotificationEntity notification)
        {
            if (this.Context.UserId == null)
                throw new UnauthorizedAccessException();

            await EnsureConnectionOpen();
            EnsureTransaction();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "Notification_Save";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Transaction = this.Transaction;
                sqlCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output, Value = notification.Id != null ? (object)notification.Id : DBNull.Value });
                sqlCommand.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = notification.UserId });
                sqlCommand.Parameters.Add(new SqlParameter("@Type", SqlDbType.Int) { Value = notification.Type });
                sqlCommand.Parameters.Add(new SqlParameter("@RecordId", SqlDbType.UniqueIdentifier) { Value = notification.RecordId });

                using (var sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();

                    dataTable.Load(sqlDataReader);

                    var dataRow = dataTable?.AsEnumerable()?.FirstOrDefault();

                    if (dataRow != null)
                    {
                        var notfication = new NotificationEntity();

                        notfication.Id = dataRow.Field<Guid>("Id");
                        notfication.UserId = dataRow.Field<Guid>("UserId");
                        notfication.CreatedDate = dataRow.Field<DateTime>("CreatedDate");
                        notfication.Type = dataRow.Field<NotificationType>("Type");
                        notfication.RecordId = dataRow.Field<Guid?>("RecordId");

                        return notfication;
                    }
                }
            }

            return null;
        }

        public async Task<IEnumerable<NotificationEntity>> Delete(NotificationDeleteRequest request)
        {
            await EnsureConnectionOpen();
            EnsureTransaction();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "Notification_Delete";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Transaction = this.Transaction;
                sqlCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = (object)request.Id ?? DBNull.Value });
                sqlCommand.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = (object)request.UserId ?? DBNull.Value });
                sqlCommand.Parameters.Add(new SqlParameter("@Type", SqlDbType.Int) { Value = (object)request.UserId ?? DBNull.Value });
                sqlCommand.Parameters.Add(new SqlParameter("@RecordId", SqlDbType.UniqueIdentifier) { Value = (object)request.RecordId ?? DBNull.Value });

                using (var sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();

                    dataTable.Load(sqlDataReader);

                    return dataTable?.AsEnumerable()
                        .Select(dataRow =>
                        {
                            var notfication = new NotificationEntity();

                            notfication.Id = dataRow.Field<Guid>("Id");
                            notfication.UserId = dataRow.Field<Guid>("UserId");
                            notfication.CreatedDate = dataRow.Field<DateTime>("CreatedDate");
                            notfication.Type = dataRow.Field<NotificationType>("Type");
                            notfication.RecordId = dataRow.Field<Guid?>("RecordId");

                            return notfication;
                        })
                        .ToList();
                }
            }
        }
    }
}

