using System;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery;

sealed class Program
{
    private async Task ExecuteAsync(string[] args)
    {
        var verbose = GetToggle(args, "verbose");
        if (verbose)
        {
            Console.WriteLine($"Payroll Engine Database Query {GetType().Assembly.GetName().Version}");
            Console.WriteLine();
        }

        // command from program arguments
        var command = GetCommand(args);

        // help
        if (command == null)
        {
            ShowHelp();
            return;
        }

        // start command
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
                case Command.TestEmptyTable:
                    await TestEmptyTableAsync(verbose, args);
                    break;
            }

        }
        catch (Exception exception)
        {
            if (Environment.ExitCode == 0)
            {
                Environment.ExitCode = -10;
            }
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
        var scriptFile = GetArgument(args, 1);
        var connectionString = GetArgument(args, 2);
        var noCatalog = GetToggle(args, "noCatalog");
        await new QueryCommand().QueryAsync(verbose, noCatalog, scriptFile, connectionString);
    }

    private async Task TestConnectionAsync(bool verbose, string[] args)
    {
        var connectionString = GetArgument(args, 1);
        await new TestConnectionCommand().TestAsync(verbose, connectionString);
    }

    private async Task TestServerAsync(bool verbose, string[] args)
    {
        var connectionString = GetArgument(args, 1);
        var timeout = TestServerCommand.DefaultTestTimeout;
        var timeoutArg = GetArgument(args, 2);
        if (!string.IsNullOrWhiteSpace(timeoutArg))
        {
            int.TryParse(timeoutArg, out timeout);
        }
        await new TestServerCommand().TestAsync(verbose, connectionString, timeout);
    }

    private async Task TestVersionAsync(bool verbose, string[] args)
    {
        if (args.Length < 5)
        {
            ShowHelp();
            return;
        }

        var connectionString = GetArgument(args, 6);
        await new TestVersionCommand().TestAsync(verbose, new()
        {
            TableName = GetArgument(args, 1),
            MajorVersionColumnName = GetArgument(args, 2),
            MinorVersionColumnName = GetArgument(args, 3),
            SubVersionColumnName = GetArgument(args, 4),
            MinVersion = GetArgument(args, 5)
        }, connectionString);
    }

    private async Task TestEmptyTableAsync(bool verbose, string[] args)
    {
        if (args.Length < 2)
        {
            ShowHelp();
            return;
        }

        var tableName = GetArgument(args, 1);
        var connectionString = GetArgument(args, 2);
        await new TestEmptyTableCommand().TestAsync(verbose, tableName, connectionString);
    }

    private static string GetArgument(string[] args, int index)
    {
        if (args.Length < 2)
        {
            return null;
        }

        // ignore args[0]
        var argIndex = 0;
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            // ignore toggle
            if (arg.StartsWith('/') || arg.StartsWith('-'))
            {
                continue;
            }
            argIndex++;
            if (argIndex == index)
            {
                return arg;
            }
        }
        return null;
    }

    private static bool GetToggle(string[] args, string name) =>
        args.Any(x => string.Equals($"/{name}", x, StringComparison.InvariantCultureIgnoreCase) ||
                      string.Equals($"-{name}", x, StringComparison.InvariantCultureIgnoreCase));

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
        TestEmptyTableCommand.ShowHelp();
        Wait();
    }

    private static void Wait()
    {
        Console.WriteLine();
        Console.Write("Press any key...");
        Console.ReadKey(true);
    }

    static async Task Main(string[] args) =>
        await new Program().ExecuteAsync(args);
}