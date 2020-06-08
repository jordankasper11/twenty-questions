using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TwentyQuestions.Web.Configuration
{
    public class ConfigurationDatabaseSettings
    {
        public string ConnectionString { get; set; }

        public void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.ConnectionString))
                throw new InvalidOperationException($"{nameof(this.ConnectionString)} cannot be null or whitespace");

            using (var sqlConnection = new SqlConnection(this.ConnectionString))
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to connect to database with connection string provider by {nameof(this.ConnectionString)}", ex);
                }
            }
        }
    }

    public class ConfigurationAuthenticationSettings
    {
        public string SecurityKey { get; set; }

        public void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.SecurityKey))
                throw new InvalidOperationException($"{nameof(this.SecurityKey)} cannot be null or whitespace");
        }
    }

    public class ConfigurationPaths
    {
        public string Avatars { get; set; }

        public void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.Avatars))
                throw new InvalidOperationException($"{nameof(this.Avatars)} cannot be null or whitespace");

            var directory = new DirectoryInfo(this.Avatars);

            if (!directory.Exists)
                throw new DirectoryNotFoundException($"Directory at {nameof(this.Avatars)} path does not exist");
        }
    }

    public class ConfigurationSettings
    {
        public ConfigurationDatabaseSettings Database { get; set; }

        public ConfigurationAuthenticationSettings Authentication { get; set; }

        public ConfigurationPaths Paths { get; set; }

        public ConfigurationSettings()
        {
            this.Database = new ConfigurationDatabaseSettings();
            this.Authentication = new ConfigurationAuthenticationSettings();
            this.Paths = new ConfigurationPaths();
        }

        public void Validate()
        {
            this.Database.Validate();
            this.Authentication.Validate();
            this.Paths.Validate();
        }
    }
}