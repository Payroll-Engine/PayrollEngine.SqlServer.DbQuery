using Microsoft.Extensions.Configuration;
using System;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal static class ConnectionConfiguration
    {
        /// <summary>Environment variable name, containing the Payroll database connection string</summary>
        /// <remarks>duplicated from PayrollEngine.SystemSpecification</remarks>
        internal static readonly string DatabaseConnectionString = "PayrollEngineDatabase";

        internal static string GetConnectionString(string argument = null)
        {
            // priority 1: command line argument
            var connectionString = argument;

            // priority 2: application configuration
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = GetConfiguration().GetConnectionString(DatabaseConnectionString);
            }

            // priority 3: environment variable
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable(DatabaseConnectionString);
            }

            return connectionString;
        }

        private static IConfiguration GetConfiguration()
        {
            // builder
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory);

            // configuration
            IConfiguration configuration = builder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();
            return configuration;
        }

    }
}
