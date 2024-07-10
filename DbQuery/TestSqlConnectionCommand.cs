using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery;

internal sealed class TestSqlConnectionCommand : CommandBase
{
    private const int DefaultTestTimeout = 5;

    internal async Task TestAsync(bool verbose, string connectionString = null, int timeout = DefaultTestTimeout)
    {
        try
        {
            // connection string
            connectionString = await ConnectionConfiguration.GetConnectionStringAsync(connectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Environment.ExitCode = -2;
                Console.WriteLine("Invalid database connection string ");
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
                WriteTitleLine("Test SQL-Server database connection");
                Console.WriteLine($"Database:     {dataSource}");
                Console.WriteLine($"Timeout:      {timeout} seconds");
                Console.WriteLine();
            }

            var available = await TestConnectionAsync(connectionString);

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
        catch (Exception exception)
        {
            Environment.ExitCode = -2;
            Console.WriteLine(exception);
        }
    }

    /// <summary>
    /// Test if SQL server can be connected
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
        WriteTitleLine("- TestSqlConnection");
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
        Console.WriteLine("          TestSqlConnection");
        Console.WriteLine("          TestSqlConnection 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;'");
        Wait();
    }
}