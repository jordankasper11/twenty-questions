using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwentyQuestions.Data.Extensions;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;

namespace TwentyQuestions.Data.Repositories
{
    public abstract class BaseRepository : IRepository, IDisposable
    {
        public SqlConnection Connection { get; protected set; }

        public IRepositoryContext Context { get; private set; }

        protected SqlTransaction Transaction { get; set; }

        public BaseRepository(SqlConnection connection, IRepositoryContext context)
        {
            this.Connection = connection;
            this.Context = context;
        }

        public async Task Commit()
        {
            if (this.Transaction != null)
            {
                await this.Transaction.CommitAsync();
                await this.Transaction.DisposeAsync();

                this.Transaction = null;
            }
        }

        protected async Task EnsureConnectionOpen()
        {
            if (!this.Connection.State.HasFlag(ConnectionState.Open))
                await this.Connection.OpenAsync();
        }

        protected void EnsureTransaction()
        {
            if (this.Transaction == null)
                this.Transaction = this.Connection.BeginTransaction();
        }

        protected string Serialize<T>(T input)
        {
            var options = GetSerializerOptions();

            return  JsonSerializer.Serialize<T>(input, options);
        }

        protected T Deserialize<T>(string input)
        {
            var options = GetSerializerOptions();

            return JsonSerializer.Deserialize<T>(input, options);
        }

        private JsonSerializerOptions GetSerializerOptions()
        {
            var options = new JsonSerializerOptions()
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return options;
        }

        public void Dispose()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Commit();
                this.Transaction.Dispose();
            }

            this.Connection.Dispose();
        }
    }

    public abstract class BaseRepository<TEntity, TRequest> : BaseRepository, IRepository<TEntity, TRequest> where TEntity : BaseEntity, new() where TRequest : BaseRequest<TEntity>, new()
    {
        private string EntityName
        {
            get
            {
                return Regex.Replace(this.GetType().Name, "Repository$", String.Empty);
            }
        }

        public BaseRepository(SqlConnection connection, IRepositoryContext context) : base(connection, context)
        {
            this.Connection = connection;
        }

        public virtual async Task<TEntity> Get(Guid id)
        {
            var request = new TRequest();

            request.Ids = new Guid[] { id };

            var response = await Get(request);

            return response?.Items?.SingleOrDefault();
        }

        public virtual async Task<EntityResponse<TEntity>> Get(TRequest request)
        {
            var entityResponse = new EntityResponse<TEntity>(request);

            await EnsureConnectionOpen();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = $"{this.EntityName}_Get";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Transaction = this.Transaction;

                if (request.Ids != null)
                {
                    if (request.Ids.Count() > 1)
                    {
                        var dataTable = new DataTable();

                        dataTable.Columns.Add("Id", typeof(Guid));

                        request.Ids.ToList().ForEach(id =>
                        {
                            var dataRow = dataTable.NewRow();

                            dataRow["Id"] = id;

                            dataTable.Rows.Add(dataRow);
                        });

                        sqlCommand.Parameters.Add(new SqlParameter("@Ids", SqlDbType.Structured) { Value = dataTable });
                    }
                    else if (request.Ids.Any())
                        sqlCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = request.Ids.Single() });
                }

                sqlCommand.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = request.Status });
                sqlCommand.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = request.PageNumber ?? 1 });
                sqlCommand.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = request.PageSize });
                sqlCommand.Parameters.Add(new SqlParameter("@TotalRecords", SqlDbType.Int) { Direction = ParameterDirection.Output, Value = request.PageSize });

                AddGetParameters(sqlCommand.Parameters, request);

                using (var sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    var dataSet = new DataSet();

                    sqlDataAdapter.Fill(dataSet);

                    entityResponse.Items = GetEntities(dataSet);
                }

                entityResponse.TotalRecords = (int?)sqlCommand.Parameters["@TotalRecords"]?.Value ?? entityResponse.Items.Count();
            }

            return entityResponse;
        }

        protected abstract void AddGetParameters(SqlParameterCollection sqlParameters, TRequest request);

        protected virtual IEnumerable<TEntity> GetEntities(DataSet dataSet)
        {
            var entities = new List<TEntity>();
            var dataTable = dataSet.Tables[0];

            foreach (DataRow dataRow in dataTable.Rows)
            {
                var entity = new TEntity();

                entity.Id = dataRow.Field<Guid>("Id");
                entity.Status = dataRow.Field<EntityStatus>("Status");

                var trackedEntity = entity as BaseTrackedEntity;

                if (trackedEntity != null)
                {
                    trackedEntity.CreatedBy = dataRow.Field<Guid?>("CreatedBy");
                    trackedEntity.CreatedDate = dataRow.GetUtcDateTime("CreatedDate");
                    trackedEntity.ModifiedBy = dataRow.Field<Guid?>("ModifiedBy");
                    trackedEntity.ModifiedDate = dataRow.GetUtcDateTime("ModifiedDate");
                }

                PopulateEntity(entity, dataRow, dataSet);

                if (entity == null)
                    throw new NullReferenceException("Entity must not be null");

                entities.Add(entity);
            }

            return entities;
        }

        protected abstract void PopulateEntity(TEntity entity, DataRow dataRow, DataSet dataSet);

        public virtual async Task<Guid> Insert(TEntity entity)
        {
            SetBaseProperties(entity);

            await EnsureConnectionOpen();
            EnsureTransaction();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = $"{this.EntityName}_Save";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Transaction = this.Transaction;
                sqlCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                sqlCommand.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = entity.Status });

                var trackedEntity = entity as BaseTrackedEntity;

                if (trackedEntity != null)
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.UniqueIdentifier) { Value = trackedEntity.ModifiedBy });
                    sqlCommand.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime2) { Value = trackedEntity.ModifiedDate });
                }

                AddInsertParameters(sqlCommand.Parameters, entity);

                await sqlCommand.ExecuteNonQueryAsync();

                return (Guid)sqlCommand.Parameters["@Id"].Value;
            }
        }

        protected abstract void AddInsertParameters(SqlParameterCollection sqlParameters, TEntity entity);

        public virtual async Task Update(TEntity entity)
        {
            SetBaseProperties(entity);

            await EnsureConnectionOpen();
            EnsureTransaction();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = $"{this.EntityName}_Save";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Transaction = this.Transaction;
                sqlCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.InputOutput, Value = entity.Id });
                sqlCommand.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = entity.Status });

                var trackedEntity = entity as BaseTrackedEntity;

                if (trackedEntity != null)
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.UniqueIdentifier) { Value = trackedEntity.ModifiedBy });
                    sqlCommand.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime2) { Value = trackedEntity.ModifiedDate });
                }

                AddUpdateParameters(sqlCommand.Parameters, entity);

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        protected abstract void AddUpdateParameters(SqlParameterCollection sqlParameters, TEntity entity);

        public virtual async Task Delete(Guid id)
        {
            await EnsureConnectionOpen();
            EnsureTransaction();

            using (var sqlCommand = this.Connection.CreateCommand())
            {
                sqlCommand.CommandText = $"{this.EntityName}_Delete";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Transaction = this.Transaction;
                sqlCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });

                var entity = new TEntity();

                SetBaseProperties(entity);

                var trackedEntity = entity as BaseTrackedEntity;

                if (trackedEntity != null)
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.UniqueIdentifier) { Value = trackedEntity.ModifiedBy });
                    sqlCommand.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime2) { Value = trackedEntity.ModifiedDate });
                }

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        private void SetBaseProperties(BaseEntity entity)
        {
            if (entity.Status == null)
                entity.Status = EntityStatus.Active;

            var trackedEntity = entity as BaseTrackedEntity;

            if (trackedEntity != null)
            {
                var currentDate = DateTime.UtcNow;

                if (trackedEntity.CreatedDate == null)
                    trackedEntity.CreatedDate = currentDate;

                if (trackedEntity.CreatedBy == null && this.Context.UserId != null)
                    trackedEntity.CreatedBy = this.Context.UserId.Value;

                trackedEntity.ModifiedDate = currentDate;
                trackedEntity.ModifiedBy = this.Context.UserId != null ? this.Context.UserId.Value : (Guid?)null;
            }
        }
    }
}
