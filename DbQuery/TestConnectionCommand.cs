using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestConnectionCommand : CommandBase
    {
        internal async Task TestAsync(string connectionString = null)
        {
            // connection string
            connectionString = GetConnectionString(connectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine($"Missing database connection string {DatabaseConnectionString}");
                Wait();
                return;
            }

            // database connection
            // // see https://stackoverflow.com/questions/10550541/how-to-get-database-name-from-connection-string-using-sqlconnectionstringbuilder
            var connectionInfo = new SqlConnectionStringBuilder(connectionString);

            // user info
            WriteTitleLine("Test database connection");
            Console.WriteLine($"Server:       {connectionInfo["Data Source"]}");
            Console.WriteLine();

            try
            {
                var available = await TestConnectionAsync(connectionString);

                Console.WriteLine();
                if (available)
                {
                    WriteSuccessLine($"SQL Server {connectionInfo["Data Source"]} is available.");
                }
                else
                {
                    Environment.ExitCode = -1;
                    WriteErrorLine($"SQL Server {connectionInfo["Data Source"]} is not available.");
                }
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Test that the server is connected
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <returns>true if the connection is opened</returns>
        /// <remarks>source: https://stackoverflow.com/a/16171261</remarks>
        private static async Task<bool> TestConnectionAsync(string connectionString)
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
            Console.WriteLine("      Test SQL database server connection");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. Database connection string (optional, default from app-settings)");
            Console.WriteLine("      Output:");
            Console.WriteLine("          Exit code -1: invalid database connection");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          TestConnection 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;'");
            Wait();
        }
    }
}
