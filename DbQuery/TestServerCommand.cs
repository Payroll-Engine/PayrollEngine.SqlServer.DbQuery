using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestServerCommand : CommandBase
    {
        public const int DefaultQueryTimeout = 5;

        internal async Task TestAsync(string connectionString = null, int timeout = DefaultQueryTimeout)
        {
            // connection string
            connectionString = ConnectionConfiguration.GetConnectionString(connectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine($"Missing database connection string {ConnectionConfiguration.DatabaseConnectionString}");
                Wait();
                return;
            }

            // database connection
            // // see https://stackoverflow.com/questions/10550541/how-to-get-database-name-from-connection-string-using-sqlconnectionstringbuilder
            var connectionInfo = new SqlConnectionStringBuilder(connectionString);
            var dataSource = connectionInfo["Data Source"] as string;

            // user info
            WriteTitleLine("Test server");
            Console.WriteLine($"Server:       {dataSource}");
            Console.WriteLine();

            try
            {
                var available = await TestServerAvailableAsync(timeout, connectionString);

                Console.WriteLine();
                if (available)
                {
                    WriteSuccessLine($"SQL Server {dataSource} is available.");
                }
                else
                {
                    Environment.ExitCode = -1;
                    WriteErrorLine($"SQL Server {dataSource} is not available.");
                }
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Test for available SQL server
        /// </summary>
        /// <param name="timeout">The request timeout</param>
        /// <param name="connectionString">The connection string</param>
        /// <remarks>source: https://www.nilebits.com/blog/2010/08/how-to-check-sql-server-database-is-exists-using-c-net/</remarks>
        /// <returns>true if the connection is opened</returns>
        private static async Task<bool> TestServerAvailableAsync(int timeout, string connectionString)
        {
            try
            {
                connectionString = SetConnectionTimeout(connectionString, timeout);

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

        /// <remarks>source: https://stackoverflow.com/a/49147314</remarks>
        private static string SetConnectionTimeout(string connectionString, int timeout)
        {
            var index = connectionString.IndexOf("Connection Timeout=", StringComparison.InvariantCultureIgnoreCase);
            if (index <= 0)
            {
                return connectionString;
            }

            var oldTimeoutString = new string(connectionString.Substring(index).Split('=')[1].Where(char.IsDigit).ToArray());
            if (!int.TryParse(oldTimeoutString, out var oldTimeout))
            {
                return connectionString;
            }
            if (oldTimeout == timeout)
            {
                return connectionString;
            }
            return connectionString.Replace($"Connection Timeout={oldTimeout}", $"Connection Timeout={timeout}");
        }

        internal static void ShowHelp()
        {
            WriteTitleLine("- TestServer");
            Console.WriteLine("      Test SQL database server");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. Database connection string (optional, default from app-settings)");
            Console.WriteLine("          2. Request timeout in seconds (optional, default: 5)");
            Console.WriteLine("      Output:");
            Console.WriteLine("          Exit code -1: database server not available");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          TestServer 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;' 2");
            Wait();
        }
    }
}
