using System;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal class CommandBase
    {
        /// <summary>Environment variable name, containing the Payroll database connection string</summary>
        /// <remarks>duplicated from PayrollEngine.SystemSpecification</remarks>
        protected static readonly string DatabaseConnectionString = "PayrollEngineDatabase";

        protected static string GetConnectionString(string value = null) => !string.IsNullOrWhiteSpace(value) ?
                value :
                GetConfiguration().GetConnectionString(DatabaseConnectionString);

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

        protected static void WriteTitleLine(string text) =>
            WriteColorLine(text, ConsoleColor.Cyan);

        protected static void WriteSuccessLine(string text) =>
            WriteColorLine(text, ConsoleColor.Green);

        protected static void WriteErrorLine(string text) =>
            WriteColorLine(text, ConsoleColor.Red);

        private static void WriteColorLine(string text, ConsoleColor color)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = foregroundColor;
        }

        protected static void Wait()
        {
            Console.WriteLine();
            Console.Write("Press any key...");
            Console.ReadKey(true);
        }
    }
}
