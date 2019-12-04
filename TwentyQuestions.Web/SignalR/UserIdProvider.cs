using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwentyQuestions.Web.SignalR
{
    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection?.User?.Claims?.SingleOrDefault(c => c.Type == "userId")?.Value;
        }
    }
}
