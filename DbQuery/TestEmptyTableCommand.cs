using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery;

internal sealed class TestEmptyTableCommand : CommandBase
{
    public const int DefaultQueryTimeout = 5;

    internal async Task TestAsync(bool verbose, string tableName, string connectionString = null)
    {
        // connection string
        connectionString = await ConnectionConfiguration.GetConnectionStringAsync(connectionString);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Environment.ExitCode = -2;
            Console.WriteLine("Missing database connection string");
            return;
        }

        // database connection
        // // see https://stackoverflow.com/questions/10550541/how-to-get-database-name-from-connection-string-using-sqlconnectionstringbuilder
        var connectionInfo = new SqlConnectionStringBuilder(connectionString);
        var dataSource = connectionInfo["Data Source"] as string;

        // user info
        if (verbose)
        {
            WriteTitleLine("Test version");
            Console.WriteLine($"Server:       {dataSource}");
            Console.WriteLine($"Table:        {tableName}");
            Console.WriteLine();
        }

        try
        {
            var count = await GetTableRowCountAsync(tableName, connectionString);
            if (count is null or 0)
            {
                Environment.ExitCode = -1;
                if (verbose)
                {
                    Console.WriteLine();
                    WriteErrorLine($"Empty table {tableName}.");
                    Console.WriteLine();
                }
                return;
            }

            // server available
            if (verbose)
            {
                Console.WriteLine();
                WriteSuccessLine($"Table {tableName} has {count} rows.");
                Console.WriteLine();
            }
        }
        catch (Exception exception)
        {
            Environment.ExitCode = -2;
            Console.WriteLine(exception);
        }
    }

    private static async Task<int?> GetTableRowCountAsync(string tableName, string connectionString)
    {
        try
        {
            var connection = new SqlConnection(connectionString);
            var query = $"SELECT COUNT(*) FROM {tableName}";
            await using (connection)
            {
                await using var command = new SqlCommand(query, connection);
                await connection.OpenAsync();

                var count = (int?)command.ExecuteScalar();
                return count;
            }
        }
        catch
        {
            return 0;
        }
    }

    internal static void ShowHelp()
    {
        WriteTitleLine("- TestEmptyTable");
        Console.WriteLine("      Test if a database server table is empty");
        Console.WriteLine("      Arguments:");
        Console.WriteLine("          1. The table name");
        Console.WriteLine("          2. Database connection string (optional, default from app-settings)");
        Console.WriteLine("      Toggles:");
        Console.WriteLine("          verbose mode: /verbose (default: off)");
        Console.WriteLine("      Output:");
        Console.WriteLine("          Exit code -1: table is empty");
        Console.WriteLine("          Exit code -2: invalid database connection string");
        Console.WriteLine("      Examples:");
        Console.WriteLine("          TestEmptyTable Tenant");
        Wait();
    }
}