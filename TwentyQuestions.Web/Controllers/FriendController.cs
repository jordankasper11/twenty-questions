using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Configuration;
using TwentyQuestions.Web.SignalR;

namespace TwentyQuestions.Web.Controllers
{
    [Route("api/Friend")]
    public class FriendController : BaseController<IFriendRepository, FriendEntity, FriendRequest>
    {
        private IHubContext<NotificationHub> _notificationHub;

        public FriendController(IFriendRepository repository, ConfigurationSettings configurationSettings, IHubContext<NotificationHub> notificationHub) : base(repository, configurationSettings)
        {
            _notificationHub = notificationHub;
        }

        public override async Task<ActionResult<FriendEntity>> Post([FromBody] FriendEntity entity)
        {
            var returnValue = await base.Post(entity);
            var friend = returnValue.Value;

            await _notificationHub.UpdateFriendsList(entity.FriendId, this.UserId.Value);

            return returnValue;
        }

        public override Task<ActionResult<FriendEntity>> Put([FromBody] FriendEntity entity)
        {
            throw new NotImplementedException();
        }

        [HttpGet("AcceptInvitation")]
        public async Task<ActionResult> AcceptInvitation(Guid id)
        {
            var friend = await this.Repository.Get(id);

            await this.Repository.AcceptInvitation(id);
            await _notificationHub.UpdateFriendsList(friend.CreatedBy.Value, this.UserId.Value);

            return Ok();
        }

        [HttpGet("DeclineInvitation")]
        public async Task<ActionResult> DeclineInvitation(Guid id)
        {
            var friend = await this.Repository.Get(id);

            await this.Repository.DeclineInvitation(id);
            await _notificationHub.UpdateFriendsList(friend.CreatedBy.Value, this.UserId.Value);

            return Ok();
        }
    }
}
