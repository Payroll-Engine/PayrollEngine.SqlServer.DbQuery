using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class QueryCommand : CommandBase
    {
        internal async Task QueryAsync(string scriptFileName, string connectionString = null)
        {
            // argument T-SQL script file name
            if (!File.Exists(scriptFileName))
            {
                Console.WriteLine($"Missing script file {scriptFileName}");
                Wait();
                return;
            }

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

            // user info
            WriteTitleLine("Database Query");
            Console.WriteLine($"Script:       {scriptFileName}");
            Console.WriteLine($"Server:       {connectionInfo["Data Source"]}");
            Console.WriteLine($"Database:     {connectionInfo["Initial Catalog"]}");
            Console.WriteLine();

            try
            {
                // read script from file
                var script = await File.ReadAllTextAsync(scriptFileName);

                Console.Write($"Executing T-SQL script ({script.Length} characters)...");

                // script execution
                // complex sql script including GO statements
                // SqlCommand doesn't support complex scripts
                // see https://stackoverflow.com/questions/40814/execute-a-large-sql-script-with-go-commands
                await using var connection = new SqlConnection(connectionString);
                var server = new Server(new ServerConnection(connection));
                var result = server.ConnectionContext.ExecuteNonQuery(script);

                Console.WriteLine();
                WriteSuccessLine($"done with query result: {result}");
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        internal static void ShowHelp()
        {
            WriteTitleLine("- Query");
            Console.WriteLine("      Execute a database T-SQL script");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. Script file name");
            Console.WriteLine("          2. Database connection string (optional, default from app-settings)");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          Query MyQuery.sql");
            Console.WriteLine("          Query MyQuery.sql 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;'");
        }
    }
}
