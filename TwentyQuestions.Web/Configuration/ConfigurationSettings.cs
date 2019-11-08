﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwentyQuestions.Web.Configuration
{
    public class ConfigurationDatabaseSettings
    {
        public string ConnectionString { get; set; }
    }

    public class ConfigurationAuthenticationSettings
    {
        public string SecurityKey { get; set; }
    }

    public class ConfigurationSettings
    {
        public ConfigurationDatabaseSettings Database { get; set; }

        public ConfigurationAuthenticationSettings Authentication { get; set; }
    }
}
