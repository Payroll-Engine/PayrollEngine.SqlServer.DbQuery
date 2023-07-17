using Microsoft.Data.SqlClient;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestConnectionCommand : CommandBase
    {
        private const int DefaultTestTimeout = 5;

        internal async Task TestAsync(bool verbose, string connectionString = null, int timeout = DefaultTestTimeout)
        {
            // connection type
            var httpConnection = !string.IsNullOrWhiteSpace(connectionString) &&
                                 connectionString.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
            try
            {
                if (httpConnection)
                {
                    await TestUrlAsync(verbose, connectionString, timeout);
                }
                else
                {
                    await TestSqlAsync(verbose, connectionString, timeout);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task TestUrlAsync(bool verbose, string url, int timeout)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(nameof(url));
            }

            // user info
            if (verbose)
            {
                WriteTitleLine("Test url connection");
                Console.WriteLine($"Url:          {url}");
                Console.WriteLine($"Timeout:      {timeout} seconds");
                Console.WriteLine();
            }

            var available = await TestWebConnectionAsync(url, timeout);

            // connection not available
            if (!available)
            {
                Environment.ExitCode = -1;
                if (verbose)
                {
                    Console.WriteLine();
                    WriteErrorLine($"Url {url} is not available.");
                    Console.WriteLine();
                }
                return;
            }

            // connection available
            if (verbose)
            {
                Console.WriteLine();
                WriteSuccessLine($"Url {url} is available.");
                Console.WriteLine();
            }
        }

        private async Task TestSqlAsync(bool verbose, string connectionString, int timeout)
        {
            // connection string
            connectionString = ConnectionConfiguration.GetConnectionString(connectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Environment.ExitCode = -2;
                if (verbose)
                {
                    Console.WriteLine($"Invalid database connection string {ConnectionConfiguration.DatabaseConnectionString}");
                    Wait();
                }
                return;
            }

            // adjust timeout
            connectionString = connectionString.SetConnectionTimeout(timeout);

            // database connection
            // // see https://stackoverflow.com/questions/10550541/how-to-get-database-name-from-connection-string-using-sqlconnectionstringbuilder
            var connectionInfo = new SqlConnectionStringBuilder(connectionString);
            var dataSource = connectionInfo["Data Source"] as string;

            // user info
            if (verbose)
            {
                WriteTitleLine("Test database connection");
                Console.WriteLine($"Database:     {dataSource}");
                Console.WriteLine($"Timeout:      {timeout} seconds");
                Console.WriteLine();
            }

            var available = await TestSqlConnectionAsync(connectionString);

            // connection not available
            if (!available)
            {
                Environment.ExitCode = -1;
                if (verbose)
                {
                    Console.WriteLine();
                    WriteErrorLine($"SQL Server {dataSource} is not available.");
                    Console.WriteLine();
                }
                return;
            }

            // connection available
            if (verbose)
            {
                Console.WriteLine();
                WriteSuccessLine($"SQL Server {dataSource} is available.");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Test if web url can be connected
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="timeout">The request timeout</param>
        /// <returns>true if the connection is opened</returns>
        /// <remarks>source: https://stackoverflow.com/a/16171261</remarks>
        private static async Task<bool> TestWebConnectionAsync(string connectionString, int timeout)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeout);
                var result = await client.GetAsync(connectionString);
                switch (result.StatusCode)
                {
                    case HttpStatusCode.Accepted:
                    case HttpStatusCode.OK:
                        return true;
                    default:
                        return false;
                }
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Test if SQL server can be connected
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <returns>true if the connection is opened</returns>
        /// <remarks>source: https://stackoverflow.com/a/16171261</remarks>
        private static async Task<bool> TestSqlConnectionAsync(string connectionString)
        {
            await using var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        internal static void ShowHelp()
        {
            WriteTitleLine("- TestConnection");
            Console.WriteLine("      Test SQL database server or Web connection");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. Database connection string (optional, default from app-settings)");
            Console.WriteLine("          2. Request timeout in seconds (optional, default: 5 seconds)");
            Console.WriteLine("      Toggles:");
            Console.WriteLine("          verbose mode: /verbose (default: off)");
            Console.WriteLine("      Output:");
            Console.WriteLine("          Exit code -1: invalid database/url connection");
            Console.WriteLine("          Exit code -2: invalid database connection string");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          TestConnection 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;'");
            Console.WriteLine("          TestConnection http://localhost:43345");
            Wait();
        }
    }
}
