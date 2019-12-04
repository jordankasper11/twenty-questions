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
        public static Task UpdateGame(this IHubContext<NotificationHub> context, Guid gameId, Guid userId)
        {
            return context.Clients.User(userId.ToString()).SendAsync("UpdateGame", gameId.ToString());
        }
    }
}
