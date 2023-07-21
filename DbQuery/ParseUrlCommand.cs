using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class ParseUrlCommand : CommandBase
    {
        internal static async Task ParseAsync(string variableName, string expression)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new ArgumentException(nameof(variableName));
            }
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException(nameof(expression));
            }

            expression = await SharedConfiguration.ParseAsync(expression);
            SetUserVariable(variableName, expression);
        }

        private static void SetUserVariable(string variableName, string variableValue) =>
            Environment.SetEnvironmentVariable(variableName, variableValue, EnvironmentVariableTarget.User);

        internal static void ShowHelp()
        {
            WriteTitleLine("- ParseUrl");
            Console.WriteLine("      Pars web url and store the result to an environment variable");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. variable name");
            Console.WriteLine("          2. Url with support for shared config variables");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          ParseUrl $BackendUrl$:$BackendPort$/");
            Console.WriteLine("          ParseUrl $WebAppUrl$:$WebAppPort$/");
            Wait();
        }
    }
}
