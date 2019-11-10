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
    [Route("api/Friend")]
    public class FriendController : BaseController<IFriendRepository, FriendEntity, FriendRequest>
    {
        public FriendController(IFriendRepository repository, ConfigurationSettings configurationSettings) : base(repository, configurationSettings)
        {
        }
    }
}
