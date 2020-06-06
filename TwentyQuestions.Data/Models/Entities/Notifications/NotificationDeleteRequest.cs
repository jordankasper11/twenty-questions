using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TwentyQuestions.Data.Models.Requests;

namespace TwentyQuestions.Data.Models.Entities
{
    public class NotificationDeleteRequest : BaseRequest
    {
        public Guid? Id { get; set; }

        public Guid? UserId { get; set; }

        public NotificationType? Type { get; set; }

        public Guid? RecordId { get; set; }

        public NotificationDeleteRequest()
        {
        }

        public NotificationDeleteRequest(Guid id)
        {
            this.Id = id;
        }

        public NotificationDeleteRequest(Guid? userId = null, NotificationType? type = null, Guid? recordId = null)
        {
            this.UserId = userId;
            this.Type = type;
            this.RecordId = recordId;
        }
    }
}