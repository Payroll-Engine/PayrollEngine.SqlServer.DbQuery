using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery;

sealed class Program
{
    private async Task ExecuteAsync(string[] args)
    {
        Console.WriteLine($"Payroll Engine Database Query {GetType().Assembly.GetName().Version}");
        Console.WriteLine();

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
                    await QueryAsync(args);
                    break;
                case Command.TestConnection:
                    await TestConnectionAsync(args);
                    break;
                case Command.TestServer:
                    await TestServerAsync(args);
                    break;
                case Command.TestVersion:
                    await TestVersionAsync(args);
                    break;
            }

        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

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

    private async Task QueryAsync(string[] args)
    {
        if (args.Length < 2)
        {
            ShowHelp();
            return;
        }
        var scriptFile = args[1];
        var connectionString = args.Length > 2 ? args[2] : null;
        await new QueryCommand().QueryAsync(scriptFile, connectionString);
    }

    private async Task TestConnectionAsync(string[] args)
    {
        var connectionString = args.Length > 1 ? args[1] : null;
        await new TestConnectionCommand().TestAsync(connectionString);
    }

    private async Task TestServerAsync(string[] args)
    {
        var timeout = args.Length > 1 ? int.Parse(args[1]) : TestServerCommand.DefaultQueryTimeout;
        var connectionString = args.Length > 2 ? args[2] : null;
        await new TestServerCommand().TestAsync(timeout, connectionString);
    }

    private async Task TestVersionAsync(string[] args)
    {
        if (args.Length < 5)
        {
            ShowHelp();
            return;
        }

        var tableName = args[1];
        var majorVersionColumnName = args[2];
        var minorVersionColumnName = args[3];
        var subVersionColumnName = args[4];
        var minVersion = args[5];
        var connectionString = args.Length > 6 ? args[6] : null;
        await new TestVersionCommand().TestAsync(tableName, majorVersionColumnName,
            minorVersionColumnName, subVersionColumnName, minVersion, connectionString);
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