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
        private INotificationRepository NotificationRepository { get; set; }
        private IHubContext<NotificationHub> NotificationHub { get; set; }

        public FriendController(IFriendRepository friendRepository, INotificationRepository notificationRepository, ConfigurationSettings configurationSettings, IHubContext<NotificationHub> notificationHub) : base(friendRepository, configurationSettings)
        {
            this.NotificationRepository = notificationRepository;
            this.NotificationHub = notificationHub;
        }

        public override async Task<ActionResult<FriendEntity>> Post([FromBody] FriendEntity entity)
        {
            var returnValue = await base.Post(entity);

            if (returnValue.Result is CreatedResult createdResult)
            {
                var friendship = (FriendEntity)createdResult.Value;
                var notification = new NotificationEntity(friendship.FriendId, NotificationType.Friend, friendship.Id.Value);

                notification = await this.NotificationRepository.Insert(notification);

                await this.NotificationHub.SendNotifications(notification);
            }

            return returnValue;
        }

        public override Task<ActionResult<FriendEntity>> Put([FromBody] FriendEntity entity)
        {
            throw new NotImplementedException();
        }

        public override async Task<ActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var friendship = await this.Repository.Get(id);

            if (friendship == null)
                return NotFound();

            await this.Repository.Delete(id);
            await this.NotificationHub.FriendsListUpdated(friendship.CreatedBy.Value, friendship.FriendId);

            return NoContent();
        }

        [HttpGet("AcceptInvitation")]
        public async Task<ActionResult> AcceptInvitation(Guid id)
        {
            var friendship = await this.Repository.Get(id);

            if (friendship == null)
                return BadRequest();

            await this.Repository.AcceptInvitation(id);

            var deletedNotifications = await this.NotificationRepository.Delete(new NotificationDeleteRequest(type: NotificationType.Friend, recordId: friendship.Id.Value));

            await this.NotificationHub.RemoveNotifications(deletedNotifications?.ToArray());
            await this.NotificationHub.FriendsListUpdated(friendship.FriendId);

            return Ok();
        }

        [HttpGet("DeclineInvitation")]
        public async Task<ActionResult> DeclineInvitation(Guid id)
        {
            var friendship = await this.Repository.Get(id);

            if (friendship == null)
                return BadRequest();

            await this.Repository.DeclineInvitation(id);

            var deletedNotifications = await this.NotificationRepository.Delete(new NotificationDeleteRequest(type: NotificationType.Friend, recordId: friendship.Id.Value));

            await this.NotificationHub.RemoveNotifications(deletedNotifications?.ToArray());
            await this.NotificationHub.FriendsListUpdated(friendship.FriendId);

            return Ok();
        }
    }
}
