using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;

namespace TwentyQuestions.Data.Repositories
{
    public interface IUserRepository : IRepository<UserEntity, UserRequest>
    {
        Task<bool> GetUsernameAvailability(string username);
    }

    public class UserRepository : BaseRepository<UserEntity, UserRequest>, IUserRepository
    {
        public UserRepository(SqlConnection connection, IRepositoryContext context) : base(connection, context)
        {
        }

        public override async Task<Guid> Insert(UserEntity entity)
        {
            try
            {
                return await base.Insert(entity);
            }
            catch (SqlException ex)
            {
                HandleSqlException(entity, ex);

                throw;
            }
        }

        public override async Task Update(UserEntity entity)
        {
            try
            {
                await base.Update(entity);
            }
            catch (SqlException ex)
            {
                HandleSqlException(entity, ex);

                throw;
            }
        }

        public async Task<bool> GetUsernameAvailability(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Username cannot be null or whitespace");

            await EnsureConnectionOpen();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = "User_GetUsernameAvailability";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar) { Value = username });

                return (bool)await sqlCommand.ExecuteScalarAsync();
            }
        }

        private void HandleSqlException(UserEntity entity, SqlException ex)
        {
            if (ex.Message.Contains("Violation of UNIQUE KEY constraint 'UC_Users'"))
                throw new DuplicateNameException($"Username '{entity.Username}' is already registered. Please try another username.");
        }

        protected override void AddGetParameters(SqlParameterCollection sqlParameters, UserRequest request)
        {
            sqlParameters.Add("@Username", SqlDbType.NVarChar).Value = request.Username;
        }

        protected override void PopulateEntity(UserEntity entity, DataRow dataRow, DataSet dataSet)
        {
            entity.Username = dataRow.Field<string>("Username");
            entity.Email = dataRow.Field<string>("Email");
            entity.AvatarFileExtension = dataRow.Field<string>("AvatarFileExtension");
        }

        protected override void AddInsertParameters(SqlParameterCollection sqlParameters, UserEntity entity)
        {
            sqlParameters.Add("@Username", SqlDbType.NVarChar).Value = entity.Username;
            sqlParameters.Add("@Email", SqlDbType.NVarChar).Value = entity.Username;
            sqlParameters.Add("@AvatarFileExtension", SqlDbType.NVarChar).Value = entity.AvatarFileExtension;
        }

        protected override void AddUpdateParameters(SqlParameterCollection sqlParameters, UserEntity entity)
        {
            sqlParameters.Add("@Username", SqlDbType.NVarChar).Value = entity.Username;
            sqlParameters.Add("@Email", SqlDbType.NVarChar).Value = entity.Username;
            sqlParameters.Add("@AvatarFileExtension", SqlDbType.NVarChar).Value = entity.AvatarFileExtension;
        }
    }
}
