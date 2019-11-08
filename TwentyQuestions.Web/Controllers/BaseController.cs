using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Models.Entities;
using TwentyQuestions.Data.Models.Requests;
using TwentyQuestions.Data.Models.Responses;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Configuration;

namespace TwentyQuestions.Web.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public abstract class BaseController<TRepository> : Controller where TRepository : IRepository
    {
        protected TRepository Repository { get; private set; }
        protected ConfigurationSettings ConfigurationSettings { get; private set; }

        public BaseController(TRepository repository, ConfigurationSettings configurationSettings)
        {
            this.Repository = repository;
            this.ConfigurationSettings = configurationSettings;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.Repository.Context.UserId = User.FindFirst("userId") != null ? Guid.Parse(User.FindFirst("userId").Value) : (Guid?)null;

            base.OnActionExecuting(context);
        }
    }

    public abstract class BaseController<TRepository, TEntity, TRequest> : BaseController<TRepository> where TRepository : IRepository<TEntity, TRequest> where TEntity : BaseEntity, new() where TRequest : BaseRequest<TEntity>
    {
        public BaseController(TRepository repository, ConfigurationSettings configurationSettings) : base(repository, configurationSettings)
        {
        }

        [HttpGet("{id:guid}")]
        public virtual async Task<ActionResult<TEntity>> Get(Guid id)
        {
            var entity = await this.Repository.Get(id);

            if (entity != null)
                return Ok(entity);

            return NotFound();
        }

        [HttpGet]
        public virtual async Task<ActionResult<EntityResponse<TEntity>>> Get(TRequest request = null)
        {
            var entities = await this.Repository.Get(request);

            return Ok(entities);
        }

        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> Post([FromBody]TEntity entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await this.Repository.Insert(entity);

            entity = await this.Repository.Get(id);

            return Created($"{Request.Path}/{entity.Id.Value}", entity);
        }

        [HttpPut]
        public virtual async Task<ActionResult<TEntity>> Put([FromBody]TEntity entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await this.Repository.Update(entity);

            entity = await this.Repository.Get(entity.Id.Value);

            return Ok(entity);
        }

        [HttpDelete]
        public virtual async Task<ActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await this.Repository.Delete(id);

            return NoContent();
        }
    }
}
