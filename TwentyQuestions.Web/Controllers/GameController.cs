using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Configuration;

namespace TwentyQuestions.Web.Controllers
{
    [Route("api/Game")]
    public class GameController : BaseController<IGameRepository, GameEntity, GameRequest>
    {
        public GameController(IGameRepository repository, ConfigurationSettings configurationSettings) : base(repository, configurationSettings)
        {
        }

        [HttpGet("AcceptInvitation")]
        public async Task<ActionResult> AcceptInvitation(Guid id)
        {
            await this.Repository.AcceptInvitation(id);
            
            return Ok();
        }

        [HttpGet("DeclineInvitation")]
        public async Task<ActionResult> DeclineInvitation(Guid id)
        {
            await this.Repository.DeclineInvitation(id);

            return Ok();
        }

        [HttpPost("AskQuestion")]
        public async Task<ActionResult<GameEntity>> AskQuestion([FromBody]AskQuestionRequest request)
        {
            await this.Repository.AskQuestion(request);

            var game = await this.Repository.Get(request.GameId);

            return Ok(game);
        }

        [HttpPost("AnswerQuestion")]
        public async Task<ActionResult<GameEntity>> AnswerQuestion([FromBody]AnswerQuestionRequest request)
        {
            await this.Repository.AnswerQuestion(request);

            var game = await this.Repository.Get(request.GameId);

            return Ok(game);
        }
    }
}