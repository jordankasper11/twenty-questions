using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public enum NotificationType
    {
        Friend,
        Game
    }

    public class NotificationEntity
    {
        [Required]
        public Guid? Id { get; set; }

        [Required]
        public Guid? UserId { get; set; }

        [Required]
        public NotificationType? Type { get; set; }

        public Guid? RecordId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public NotificationEntity()
        {
        }

        public NotificationEntity(Guid userId, NotificationType type, Guid? recordId = null)
        {
            this.Id = Guid.NewGuid();
            this.UserId = userId;
            this.Type = type;
            this.RecordId = recordId;
            this.CreatedDate = DateTime.UtcNow;
        }
    }
}