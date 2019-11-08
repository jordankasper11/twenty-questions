using System;
using System.Collections.Generic;
using System.Text;
using TwentyQuestions.Data.Models.Entities;

namespace TwentyQuestions.Data.Models.Requests
{
    public enum SortDirection
    {
        ASC,
        DESC
    }

    public abstract class BaseRequest
    {
    }

    public abstract class BaseRequest<TEntity> where TEntity : BaseEntity
    {
        public IEnumerable<Guid> Ids { get; set; }

        public string SortBy { get; set; }

        public SortDirection SortDirection { get; set; }

        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }

        public BaseRequest()
        {
            this.SortDirection = SortDirection.ASC;
            this.PageNumber = 1;
        }
    }
}
