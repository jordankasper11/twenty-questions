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
    }
}
