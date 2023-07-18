using Microsoft.Extensions.Configuration;
using System;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal static class ConnectionConfiguration
    {
        /// <summary>Setting name, containing the Payroll database connection string</summary>
        private static readonly string DatabaseConnectionSetting = "DatabaseConnection";

        internal static string GetConnectionString(string argument = null)
        {
            // priority 1: command line argument
            var connectionString = argument;

            // priority 2: application configuration
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = GetConfiguration().GetConnectionString(DatabaseConnectionSetting);
            }

            // priority 3: shared configuration
            var sharedConfiguration = SharedConfiguration.GetSharedConfiguration();
            if (sharedConfiguration.TryGetValue(DatabaseConnectionSetting, out var value))
            {
                connectionString = value;
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
