using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    [Flags]
    public enum EntityStatus
    {
        Active = 1,
        Pending = 2,
        Archived = 4,
        Deleted = 8
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
