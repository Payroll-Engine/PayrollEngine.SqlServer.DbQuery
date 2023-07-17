using System;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery;

sealed class Program
{
    private async Task ExecuteAsync(string[] args)
    {
        var verbose = VerboseMode(args);
        if (verbose)
        {
            Console.WriteLine($"Payroll Engine Database Query {GetType().Assembly.GetName().Version}");
            Console.WriteLine();
        }

        // command
        var command = GetCommand(args);
        if (command == null)
        {
            ShowHelp();
            return;
        }
        try
        {
            switch (command)
            {
                case Command.Query:
                    await QueryAsync(verbose, args);
                    break;
                case Command.TestConnection:
                    await TestConnectionAsync(verbose, args);
                    break;
                case Command.TestServer:
                    await TestServerAsync(verbose, args);
                    break;
                case Command.TestVersion:
                    await TestVersionAsync(verbose, args);
                    break;
            }

        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private async Task QueryAsync(bool verbose, string[] args)
    {
        if (args.Length < 2)
        {
            ShowHelp();
            return;
        }
        var scriptFile = args[1];
        var connectionString = args.Length > 2 ? args[2] : null;
        await new QueryCommand().QueryAsync(verbose, NoCatalogMode(args), scriptFile, connectionString);
    }
    
    private static bool NoCatalogMode(string[] args) =>
        args.Any(x => string.Equals("/noCatalog", x, StringComparison.InvariantCultureIgnoreCase));

    private async Task TestConnectionAsync(bool verbose, string[] args)
    {
        var connectionString = args.Length > 1 ? args[1] : null;
        await new TestConnectionCommand().TestAsync(verbose, connectionString);
    }

    private async Task TestServerAsync(bool verbose, string[] args)
    {
        var connectionString = args.Length > 1 ? args[1] : null;
        var timeout = args.Length > 2 ? int.Parse(args[2]) : TestServerCommand.DefaultTestTimeout;
        await new TestServerCommand().TestAsync(verbose, connectionString, timeout);
    }

    private async Task TestVersionAsync(bool verbose, string[] args)
    {
        if (args.Length < 5)
        {
            ShowHelp();
            return;
        }

        var connectionString = args.Length > 6 ? args[6] : null;
        await new TestVersionCommand().TestAsync(verbose, new()
        {
            TableName = args[1],
            MajorVersionColumnName = args[2],
            MinorVersionColumnName = args[3],
            SubVersionColumnName = args[4],
            MinVersion = args[5]
        }, connectionString);
    }

    private static bool VerboseMode(string[] args) =>
        args.Any(x => string.Equals("/verbose", x, StringComparison.InvariantCultureIgnoreCase));

    private static Command? GetCommand(string[] args)
    {
        if (args.Length < 1)
        {
            return null;
        }

        var commandName = args[0];
        if (!string.IsNullOrWhiteSpace(commandName) &&
            Enum.TryParse<Command>(commandName, out var command))
        {
            return command;
        }
        return null;
    }

    private static void ShowHelp()
    {
        QueryCommand.ShowHelp();
        TestServerCommand.ShowHelp();
        TestConnectionCommand.ShowHelp();
        TestVersionCommand.ShowHelp();
        Wait();
    }

    private static void Wait()
    {
        Console.WriteLine();
        Console.Write("Press any key...");
        Console.ReadKey(true);
    }

    static async Task Main(string[] args)
    {
        try
        {
            await new Program().ExecuteAsync(args);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}