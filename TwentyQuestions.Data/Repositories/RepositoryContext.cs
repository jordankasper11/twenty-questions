using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyQuestions.Data.Repositories
{
    public class RepositoryContext : IRepositoryContext
    {
        public Guid? UserId { get; set; }
    }
}
