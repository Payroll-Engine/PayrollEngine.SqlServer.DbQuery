using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal static class ConnectionConfiguration
    {
        /// <summary>Configuration setting name, containing the payroll database connection string</summary>
        private const string ConfigurationDatabaseConnectionString = "PayrollDatabaseConnection";

        /// <summary>Shared setting name, containing the payroll database connection string</summary>
        private const string SharedDatabaseConnectionString = "DatabaseConnection";

        internal static async Task<string> GetConnectionStringAsync(string argument = null)
        {
            var connectionString = argument;

            // priority 1: command line argument
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            // priority 2: shared configuration, copy from Core ConfigurationExtensions
            var sharedConfiguration = await SharedConfiguration.ReadAsync();
            connectionString = SharedConfiguration.GetSharedValue(sharedConfiguration, SharedDatabaseConnectionString);
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            // priority 3: application configuration
            connectionString = GetConfiguration().GetConnectionString(ConfigurationDatabaseConnectionString);
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
