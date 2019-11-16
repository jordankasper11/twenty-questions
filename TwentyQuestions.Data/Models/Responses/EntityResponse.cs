using System;
using System.Collections.Generic;
using System.Text;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;

namespace TwentyQuestions.Data.Models.Responses
{
    public class EntityResponse<TEntity> : BaseResponse where TEntity : BaseEntity
    {
        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }

        public int TotalRecords { get; set; }

        public IEnumerable<TEntity> Items { get; set; }

        public EntityResponse()
        {
        }

        public EntityResponse(BaseRequest<TEntity> request)
        {
            this.PageNumber = request.PageNumber;
            this.PageSize = request.PageSize;
        }
    }
}
