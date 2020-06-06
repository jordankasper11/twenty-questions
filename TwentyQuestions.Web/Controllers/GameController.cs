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
    [Route("api/Game")]
    public class GameController : BaseController<IGameRepository, GameEntity, GameRequest>
    {
        private INotificationRepository NotificationRepository { get; set; }
        private IHubContext<NotificationHub> NotificationHub { get; set; }

        public GameController(IGameRepository gameRepository, INotificationRepository notificationRepository, ConfigurationSettings configurationSettings, IHubContext<NotificationHub> notificationHub) : base(gameRepository, configurationSettings)
        {
            this.NotificationRepository = notificationRepository;
            this.NotificationHub = notificationHub;
        }

        public override async Task<ActionResult<GameEntity>> Post([FromBody] GameEntity entity)
        {
            var returnValue = await base.Post(entity);

            if (returnValue.Result is CreatedResult createdResult)
            {
                var game = (GameEntity)createdResult.Value;
                var notification = new NotificationEntity(game.OpponentId, NotificationType.Game, game.Id.Value);

                notification = await this.NotificationRepository.Insert(notification);

                await this.NotificationHub.SendNotifications(notification);
                await this.NotificationHub.GamesListUpdated(game.CreatedBy.Value, game.OpponentId);
            }

            return returnValue;
        }

        [HttpGet("AcceptInvitation")]
        public async Task<ActionResult> AcceptInvitation(Guid id)
        {
            await this.Repository.AcceptInvitation(id);

            var game = await this.Repository.Get(id);

            // Do not delete notifications because it is still the recipient's turn to guess
            await this.NotificationHub.GamesListUpdated(game.CreatedBy.Value, game.OpponentId);

            return Ok();
        }

        [HttpGet("DeclineInvitation")]
        public async Task<ActionResult> DeclineInvitation(Guid id)
        {
            var game = await this.Repository.Get(id);

            await this.Repository.DeclineInvitation(id);
            
            var notifications = await this.NotificationRepository.Delete(new NotificationDeleteRequest(recordId: game.Id.Value));

            await this.NotificationHub.RemoveNotifications(notifications?.ToArray());
            await this.NotificationHub.GamesListUpdated(game.CreatedBy.Value, game.OpponentId);

            return Ok();
        }

        [HttpPost("AskQuestion")]
        public async Task<ActionResult<GameEntity>> AskQuestion([FromBody] AskQuestionRequest request)
        {
            await this.Repository.AskQuestion(request);

            var game = await this.Repository.Get(request.GameId);
            var deletedNotifications = await this.NotificationRepository.Delete(new NotificationDeleteRequest(type: NotificationType.Game, recordId: request.GameId));

            await this.NotificationHub.RemoveNotifications(deletedNotifications?.ToArray());

            var notification = new NotificationEntity(game.CreatedBy.Value, NotificationType.Game, game.Id.Value);

            notification = await this.NotificationRepository.Insert(notification);

            await this.NotificationHub.SendNotifications(notification);
            await this.NotificationHub.GamesListUpdated(game.CreatedBy.Value, game.OpponentId);

            return Ok(game);
        }

        [HttpPost("AnswerQuestion")]
        public async Task<ActionResult<GameEntity>> AnswerQuestion([FromBody] AnswerQuestionRequest request)
        {
            await this.Repository.AnswerQuestion(request);

            var game = await this.Repository.Get(request.GameId);
            var deletedNotifications = await this.NotificationRepository.Delete(new NotificationDeleteRequest(type: NotificationType.Game, recordId: request.GameId));

            await this.NotificationHub.RemoveNotifications(deletedNotifications?.ToArray());

            var notification = new NotificationEntity(game.OpponentId, NotificationType.Game, game.Id.Value);

            notification = await this.NotificationRepository.Insert(notification);

            await this.NotificationHub.SendNotifications(notification);
            await this.NotificationHub.GamesListUpdated(game.CreatedBy.Value, game.OpponentId);

            return Ok(game);
        }
    }
}