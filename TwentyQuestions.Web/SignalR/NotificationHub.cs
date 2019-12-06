using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwentyQuestions.Web.SignalR
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationHub : Hub
    {
    }

    public static class NotificationHubExtensions
    {
        public static Task UpdateFriendsList(this IHubContext<NotificationHub> context, params Guid[] userIds)
        {
            return context.Clients.Users(userIds.Select(i => i.ToString()).ToArray()).SendAsync("UpdateFriendsList");
        }

        public static Task UpdateGame(this IHubContext<NotificationHub> context, Guid gameId, params Guid[] userIds)
        {
            return context.Clients.Users(userIds.Select(i => i.ToString()).ToArray()).SendAsync("UpdateGame", gameId.ToString());
        }

        public static Task UpdateGamesList(this IHubContext<NotificationHub> context, params Guid[] userIds)
        {
            return context.Clients.Users(userIds.Select(i => i.ToString()).ToArray()).SendAsync("UpdateGamesList");
        }
    }
}
