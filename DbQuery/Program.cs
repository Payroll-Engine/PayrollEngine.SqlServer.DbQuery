using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace PayrollEngine.SqlServer.DbQuery;

sealed class Program
{
    /// <summary>Environment variable name, containing the Payroll database connection string</summary>
    /// <remarks>duplicated from PayrollEngine.SystemSpecification</remarks>
    private static readonly string DatabaseConnectionString = "PayrollEngineDatabase";

    private void Execute(string[] args)
    {
        Console.WriteLine($"Payroll Engine Database Query {GetType().Assembly.GetName().Version}");
        Console.WriteLine();

        if (args.Length < 1)
        {
            ShowHelp();
            return;
        }

        // argument T-SQL script file name
        var scriptFileName = args[0];
        if (!File.Exists(scriptFileName))
        {
            Console.WriteLine($"Missing script file {scriptFileName}");
            Wait();
            return;
        }

        // configuration
        var configuration = GetConfiguration();

        // connection string
        var dbConnectionString = configuration.GetConnectionString(DatabaseConnectionString);
        if (string.IsNullOrWhiteSpace(dbConnectionString) && args.Length > 1)
        {
            dbConnectionString = args[1];
        }
        if (string.IsNullOrWhiteSpace(dbConnectionString))
        {
            Console.WriteLine($"Missing database connection string {DatabaseConnectionString}");
            Wait();
            return;
        }

        // database connection
        // // see https://stackoverflow.com/questions/10550541/how-to-get-database-name-from-connection-string-using-sqlconnectionstringbuilder
        var connectionInfo = new SqlConnectionStringBuilder(dbConnectionString);

        // user info
        Console.WriteLine("Database query");
        Console.WriteLine($"Script:       {scriptFileName}");
        Console.WriteLine($"Server:       {connectionInfo["Data Source"]}");
        Console.WriteLine($"Database:     {connectionInfo["Initial Catalog"]}");
        Console.WriteLine();

        try
        {
            // read script from file
            var script = File.ReadAllText(scriptFileName);

            Console.Write($"Executing T-SQL script ({script.Length} characters)...");

            // script execution
            // complex sql script including GO statements
            // SqlCommand doesn't support complex scripts
            // see https://stackoverflow.com/questions/40814/execute-a-large-sql-script-with-go-commands
            using var connection = new SqlConnection(dbConnectionString);
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

    private static void ShowHelp()
    {
        Console.WriteLine("Usage: DbQuery ScriptFile [ConnectionString]");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  DbQuery MyQuery.sql");
        Console.WriteLine("  DbQuery MyQuery.sql 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;'");
        Wait();
    }

    private static void WriteSuccessLine(string text) =>
        WriteColorLine(text, ConsoleColor.Green);

    private static void WriteColorLine(string text, ConsoleColor color)
    {
        var foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = foregroundColor;
    }

    private static void Wait()
    {
        Console.WriteLine();
        Console.Write("Press any key...");
        Console.ReadKey(true);
    }

    static void Main(string[] args)
    {
        try
        {
            new Program().Execute(args);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}