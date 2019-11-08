using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public enum EntityStatus
    {
        Active = 1,
        Archived = 2,
        Deleted = 3
    }

    public abstract class BaseEntity
    {
        public Guid? Id { get; set; }

        public EntityStatus? Status { get; set; }
    }

    public abstract class BaseTrackedEntity : BaseEntity
    {
        public DateTime? CreatedDate { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public Guid? ModifiedBy { get; set; }
    }
}
