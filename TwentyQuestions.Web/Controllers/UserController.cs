using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Configuration;

namespace TwentyQuestions.Web.Controllers
{
    [Route("api/User")]
    public class UserController : BaseController<IUserRepository, UserEntity, UserRequest>
    {
        public UserController(IUserRepository repository, ConfigurationSettings configurationSettings) : base(repository, configurationSettings)
        {
        }

        public override Task<ActionResult<UserEntity>> Post([FromBody] UserEntity entity)
        {
            throw new NotImplementedException();
        }

        [AllowAnonymous]
        [HttpGet("GetUsernameAvailability")]
        public async Task<ActionResult<bool>> GetUsernameAvailability(string username)
        {
            var usernameAvailability = await this.Repository.GetUsernameAvailability(username);

            return Ok(usernameAvailability);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<UserEntity>> Register([FromBody] RegistrationRequest request, [FromServices] IAuthenticationRepository authenticationRepository)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new UserEntity();

            user.Username = request.Username;
            user.Email = request.Email;

            var userId = await this.Repository.Insert(user);

            await this.Repository.Commit();

            var userCredentials = authenticationRepository.HashPassword(request.Password);

            userCredentials.UserId = userId;

            await authenticationRepository.SaveUserCredentials(userCredentials);

            user = await this.Repository.Get(userId);

            return Ok(user);
        }

        [HttpPost("SaveAvatar")]
        public async Task<ActionResult<string>> SaveAvatar(Guid userId)
        {
            var file = this.Request.Form.Files?.FirstOrDefault();

            if (file == null)
                throw new InvalidOperationException("A file upload is required");

            var avatarUrl = await this.Repository.SaveAvatar(userId, file);

            return Ok(avatarUrl);
        }

        [HttpDelete("RemoveAvatar")]
        public async Task<ActionResult> RemoveAvatar(Guid userId)
        {
            await this.Repository.RemoveAvatar(userId);

            return NoContent();
        }
    }
}
