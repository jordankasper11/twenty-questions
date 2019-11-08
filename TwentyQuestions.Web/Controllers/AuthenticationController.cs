using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Configuration;

namespace TwentyQuestions.Web.Controllers
{
    [Route("api/Authentication")]
    public class AuthenticationController : BaseController<IAuthenticationRepository>
    {
        public AuthenticationController(IAuthenticationRepository repository, ConfigurationSettings configurationSettings) : base(repository, configurationSettings)
        {
        }

        [Route("Login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<string>> Login([FromBody]LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await this.Repository.Login(request);

            if (token != null)
                return Ok(token);

            return Unauthorized();
        }
    }
}
