using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyQuestions.Data.Models.Entities
{
    public class NotificationsEntity
    {
        public bool FriendNotifications { get; set; }

        public bool GameNotifications { get; set; }
    }
}
