using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery;

/// <summary>
/// complex sql script including GO statements
/// SqlCommand doesn't support complex scripts
/// <remarks>see https://stackoverflow.com/questions/40814/execute-a-large-sql-script-with-go-commands</remarks>
/// </summary>
internal sealed class QueryCommand : CommandBase
{
    internal async Task QueryAsync(bool verbose, bool noCatalog, string scriptFileMask, string connectionString = null)
    {
        // argument T-SQL script file name
        var scriptFiles = GetScriptFileNames(scriptFileMask);
        if (!scriptFiles.Any())
        {
            Environment.ExitCode = -3;
            Console.WriteLine($"Missing script file {scriptFileMask}");
            return;
        }

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

        // query without catalog
        if (noCatalog)
        {
            connectionInfo.Remove("Initial Catalog");
            connectionString = connectionInfo.ToString();
        }

        // user info
        if (verbose)
        {
            WriteTitleLine("Database Query");
            Console.WriteLine($"Script:       {scriptFileMask}");
            Console.WriteLine($"Server:       {connectionInfo["Data Source"]}");
            if (!noCatalog)
            {
                Console.WriteLine($"Database:     {connectionInfo["Initial Catalog"]}");
            }
            Console.WriteLine();
        }

        try
        {
            // script execution
            await using var connection = new SqlConnection(connectionString);
            var server = new Server(new ServerConnection(connection));

            foreach (var scriptFile in scriptFiles)
            {
                // read script from file
                var script = await File.ReadAllTextAsync(scriptFile);
                if (string.IsNullOrWhiteSpace(script))
                {
                    if (verbose)
                    {
                        WriteErrorLine($"Ignoring empty script file {scriptFile}");
                    }
                    continue;
                }

                // query execute
                if (verbose)
                {
                    Console.Write($"Executing script {scriptFile} ({script.Length} characters)...");
                }
                var result = server.ConnectionContext.ExecuteNonQuery(script);
                if (verbose)
                {
                    Console.WriteLine();
                    WriteSuccessLine($"done with query result: {result}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception exception)
        {
            Environment.ExitCode = -1;
            Console.WriteLine(exception);
        }
    }

    internal static void ShowHelp()
    {
        WriteTitleLine("- Query");
        Console.WriteLine("      Execute a database T-SQL script");
        Console.WriteLine("      Arguments:");
        Console.WriteLine("          1. Script file name(s) with file mask support");
        Console.WriteLine("          2. Database connection string (optional, default from app-settings)");
        Console.WriteLine("      Toggles:");
        Console.WriteLine("          verbose mode: /verbose (default: off)");
        Console.WriteLine("          ignore connection initial catalog: /noCatalog (default: off)");
        Console.WriteLine("      Output:");
        Console.WriteLine("          Exit code -1: error in script execution");
        Console.WriteLine("          Exit code -2: invalid database connection string");
        Console.WriteLine("          Exit code -3: invalid script file");
        Console.WriteLine("      Examples:");
        Console.WriteLine("          Query MyQuery.sql");
        Console.WriteLine("          Query MyQuery.sql 'server=localhost;database=MyDatabase; Integrated Security=SSPI;TrustServerCertificate=True;'");
    }

    private static List<string> GetScriptFileNames(string fileMask)
    {
        if (string.IsNullOrWhiteSpace(fileMask))
        {
            throw new ArgumentException(nameof(fileMask));
        }

        var files = new List<string>();
        // single file
        if (File.Exists(fileMask))
        {
            files.Add(fileMask);
        }
        else
        {
            // multiple files
            files.AddRange(new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles(fileMask).Select(x => x.Name));
        }
        return files;
    }
}