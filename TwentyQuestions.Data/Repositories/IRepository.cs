using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;

namespace TwentyQuestions.Data.Repositories
{
    public interface IRepositoryContext
    {
        Guid? UserId { get; set; }
    }

    public interface IRepository
    {
        SqlConnection Connection { get; }
        IRepositoryContext Context { get; }
    }

    public interface IRepository<TEntity, TRequest> : IRepository where TEntity : BaseEntity where TRequest : BaseRequest<TEntity>
    {
        Task Delete(Guid id);
        Task<TEntity> Get(Guid id);
        Task<EntityResponse<TEntity>> Get(TRequest request);
        Task<Guid> Insert(TEntity entity);
        Task Update(TEntity entity);
    }
}
