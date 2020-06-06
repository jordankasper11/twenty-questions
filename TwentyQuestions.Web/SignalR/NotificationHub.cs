using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Repositories;

namespace TwentyQuestions.Web.SignalR
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationHub : Hub
    {
        private INotificationRepository NotificationRepository { get; set; }

        public NotificationHub(INotificationRepository notificationRepository)
        {
            this.NotificationRepository = notificationRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = this.Context.UserIdentifier;
            var notifications = await this.NotificationRepository.Get(Guid.Parse(userId));

            await this.Clients.User(userId).SendAsync("NotificationsReceived", notifications);
        }

        public async Task RemoveNotification(Guid? id, NotificationType? type, Guid? recordId)
        {
            var request = new NotificationDeleteRequest()
            {
                Id = id,
                UserId = Guid.Parse(this.Context.UserIdentifier),
                Type = type,
                RecordId = recordId
            };

            var notifications = await this.NotificationRepository.Delete(request);

            if (notifications?.Any() == true)
                await this.Clients.User(this.Context.UserIdentifier).SendAsync("NotificationsRemoved", notifications.ToArray());
        }
    }

    public static class NotificationHubExtensions
    {
        public static async Task SendNotifications(this IHubContext<NotificationHub> context, params NotificationEntity[] notifications)
        {
            if (notifications?.Any() != true)
                return;

            foreach (var userNotifications in notifications.GroupBy(n => n.UserId))
            {
                var userId = userNotifications.Key.ToString();

                await context.Clients.User(userId).SendAsync("NotificationsReceived", userNotifications.ToArray());
            }
        }

        public static async Task RemoveNotifications(this IHubContext<NotificationHub> context, params NotificationEntity[] notifications)
        {
            if (notifications?.Any() != true)
                return;

            foreach (var userNotifications in notifications.GroupBy(n => n.UserId))
            {
                var userId = userNotifications.Key.ToString();

                await context.Clients.User(userId).SendAsync("NotificationsRemoved", userNotifications.ToArray());
            }
        }

        public static Task FriendsListUpdated(this IHubContext<NotificationHub> context, params Guid[] userIds)
        {
            return context.Clients.Users(userIds.Select(i => i.ToString()).ToArray()).SendAsync("FriendsListUpdated");
        }

        public static Task GamesListUpdated(this IHubContext<NotificationHub> context, params Guid[] userIds)
        {
            return context.Clients.Users(userIds.Select(i => i.ToString()).ToArray()).SendAsync("GamesListUpdated");
        }
    }
}
