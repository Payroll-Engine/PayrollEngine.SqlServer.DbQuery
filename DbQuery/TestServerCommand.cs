using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestServerCommand : CommandBase
    {
        internal const int DefaultTestTimeout = 5;

        internal async Task TestAsync(bool verbose, string connectionString = null, int timeout = DefaultTestTimeout)
        {
            // connection string
            connectionString = ConnectionConfiguration.GetConnectionString(connectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Environment.ExitCode = -2;
                if (verbose)
                {
                    Console.WriteLine($"Missing database connection string {ConnectionConfiguration.DatabaseConnectionString}");
                    Wait();
                }
                return;
            }

            // database connection
            // // see https://stackoverflow.com/questions/10550541/how-to-get-database-name-from-connection-string-using-sqlconnectionstringbuilder
            var connectionInfo = new SqlConnectionStringBuilder(connectionString);
            // remove initial catalog from connection string
            connectionInfo.Remove("Initial Catalog");
            connectionString = connectionInfo.ToString();
            var dataSource = connectionInfo["Data Source"] as string;

            // user info
            if (verbose)
            {
                WriteTitleLine("Test server");
                Console.WriteLine($"Server:       {dataSource}");
                Console.WriteLine($"Timeout:      {timeout} seconds");
                Console.WriteLine();
            }

            try
            {
                var available = await TestServerAvailableAsync(connectionString, timeout);

                // server not available
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

                // server available
                if (verbose)
                {
                    Console.WriteLine();
                    WriteSuccessLine($"SQL Server {dataSource} is available.");
                    Console.WriteLine();
                }
            }
            catch (Exception exception)
            {
                Environment.ExitCode = -2;
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Test for available SQL server
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="timeout">The request timeout</param>
        /// <remarks>source: https://www.nilebits.com/blog/2010/08/how-to-check-sql-server-database-is-exists-using-c-net/</remarks>
        /// <returns>true if the connection is opened</returns>
        private static async Task<bool> TestServerAvailableAsync(string connectionString, int timeout)
        {
            try
            {
                connectionString = connectionString.SetConnectionTimeout(timeout);

                var connection = new SqlConnection(connectionString);
                var query = "SELECT database_id FROM sys.databases WHERE Name = 'master'";
                await using (connection)
                {
                    await using var command = new SqlCommand(query, connection);
                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    connection.Close();

                    if (result == null)
                    {
                        return false;
                    }
                    return (int)result > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        internal static void ShowHelp()
        {
            WriteTitleLine("- TestServer");
            Console.WriteLine("      Test SQL database server");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. Database connection string (optional, default from app-settings)");
            Console.WriteLine("          2. Request timeout in seconds (optional, default: 5 seconds)");
            Console.WriteLine("      Toggles:");
            Console.WriteLine("          verbose mode: /verbose (default: off)");
            Console.WriteLine("      Output:");
            Console.WriteLine("          Exit code -1: database server not available");
            Console.WriteLine("          Exit code -2: invalid database connection string");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          TestServer 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;' 2");
            Wait();
        }
    }
}
