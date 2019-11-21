using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Utilities;

namespace TwentyQuestions.Data.Repositories
{
    public interface IUserRepository : IRepository<UserEntity, UserRequest>
    {
        Task<bool> GetUsernameAvailability(string username);

        Task<string> SaveAvatar(Guid userId, IFormFile file);

        Task RemoveAvatar(Guid userId);
    }

    public class UserRepository : BaseRepository<UserEntity, UserRequest>, IUserRepository
    {
        private string AvatarPath { get; set; }

        public UserRepository(SqlConnection connection, IRepositoryContext context, string avatarPath) : base(connection, context)
        {
            this.AvatarPath = avatarPath;
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
            if (entity.Id != this.Context.UserId)
                throw new InvalidOperationException("Users can only update their own profiles");

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

        public async Task<string> SaveAvatar(Guid userId, IFormFile file)
        {
            var user = await this.Get(userId);

            if (userId != this.Context.UserId)
                throw new InvalidOperationException("Users can only update their own profiles");

            if (String.IsNullOrWhiteSpace(this.AvatarPath))
                throw new InvalidOperationException("AvatarPath cannot be null or whitespace");

            if (!Directory.Exists(this.AvatarPath))
                Directory.CreateDirectory(this.AvatarPath);

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid file type");

            var fileExtension = MimeUtility.GetFileExtension(file.ContentType);

            if (String.IsNullOrWhiteSpace(fileExtension))
                throw new InvalidOperationException("Invalid file type");

            var filePath = Path.Combine(this.AvatarPath, $"{userId}{fileExtension}");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var fileInfo = new FileInfo(filePath);

            user.AvatarFileExtension = fileInfo.Extension;

            await this.Update(user);

            user = await this.Get(userId);

            return user.AvatarUrl;
        }

        public async Task RemoveAvatar(Guid userId)
        {
            var user = await this.Get(userId);

            if (userId != this.Context.UserId)
                throw new InvalidOperationException("Users can only update their own profiles");

            if (String.IsNullOrWhiteSpace(this.AvatarPath))
                throw new InvalidOperationException("AvatarPath cannot be null or whitespace");

            var filePath = Path.Combine(this.AvatarPath, $"{userId}.{user.AvatarFileExtension}");
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
                fileInfo.Delete();

            user.AvatarFileExtension = null;

            await this.Update(user);
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
