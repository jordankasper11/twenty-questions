using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;

namespace TwentyQuestions.Data.Repositories
{
    public interface IUserRepository : IRepository<UserEntity, UserRequest>
    {
    }

    public class UserRepository : BaseRepository<UserEntity, UserRequest>, IUserRepository
    {
        public UserRepository(SqlConnection connection, IRepositoryContext context) : base(connection, context)
        {
        }

        protected override void AddGetParameters(SqlParameterCollection sqlParameters, UserRequest request)
        {
            throw new NotImplementedException();
        }

        protected override void PopulateEntity(UserEntity entity, DataRow dataRow, DataSet dataSet)
        {
            throw new NotImplementedException();
        }

        protected override void AddInsertParameters(SqlParameterCollection sqlParameters, UserEntity entity)
        {
            throw new NotImplementedException();
        }

        protected override void AddUpdateParameters(SqlParameterCollection sqlParameters, UserEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
