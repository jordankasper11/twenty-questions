using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;
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

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody]LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loginResponse = await this.Repository.Login(request);

            if (loginResponse != null)
                return Ok(loginResponse);

            return Unauthorized("Invalid credentials");
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody]RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loginResponse = await this.Repository.RefreshToken(request);

            if (loginResponse != null)
                return Ok(loginResponse);

            return Forbid();
        }
    }
}
