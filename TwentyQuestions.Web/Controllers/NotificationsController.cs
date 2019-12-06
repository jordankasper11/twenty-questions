using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Configuration;

namespace TwentyQuestions.Web.Controllers
{
    [Route("api/Notifications")]
    public class NotificationsController : BaseController<INotificationsRepository>
    {
        public NotificationsController(INotificationsRepository repository, ConfigurationSettings configurationSettings) : base(repository, configurationSettings)
        {
        }

        [HttpGet]
        public async Task<ActionResult<NotificationsEntity>> Get(DateTime? gamesLastChecked = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notifications = await this.Repository.Get(gamesLastChecked);

            return Ok(notifications);
        }
    }
}
