using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestHttpConnectionCommand : CommandBase
    {
        // timeout in seconds
        private const int DefaultTestTimeout = 30;

        internal async Task TestAsync(bool verbose, string expression, int timeout = DefaultTestTimeout)
        {
            try
            {
                // user info
                if (verbose)
                {
                    WriteTitleLine("Test HTTP connection");
                    Console.WriteLine($"Url:          {expression}");
                    Console.WriteLine($"Timeout:      {timeout} seconds");
                    Console.WriteLine();
                }

                var available = await TestUrlAsync(expression, timeout);

                // connection not available
                if (!available)
                {
                    Environment.ExitCode = -1;
                    if (verbose)
                    {
                        Console.WriteLine();
                        WriteErrorLine($"Url {expression} is not available.");
                        Console.WriteLine();
                    }
                    return;
                }

                // connection available
                if (verbose)
                {
                    Console.WriteLine();
                    WriteSuccessLine($"Url {expression} is available.");
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
        /// Test if web url can be connected
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="timeout">The request timeout</param>
        /// <returns>true if the connection is opened</returns>
        /// <remarks>source: https://stackoverflow.com/a/16171261</remarks>
        private static async Task<bool> TestUrlAsync(string connectionString, int timeout)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeout);
                var result = await client.GetAsync(connectionString);
                switch (result.StatusCode)
                {
                    case HttpStatusCode.Accepted:
                    case HttpStatusCode.OK:
                        return true;
                    default:
                        return false;
                }
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        internal static void ShowHelp()
        {
            WriteTitleLine("- TestHttpConnection");
            Console.WriteLine("      Test HTTP connection");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. Url with support for shared config variables");
            Console.WriteLine("          2. Request timeout in seconds (optional, default: 30 seconds)");
            Console.WriteLine("      Toggles:");
            Console.WriteLine("          verbose mode: /verbose (default: off)");
            Console.WriteLine("      Output:");
            Console.WriteLine("          Exit code -1: invalid http connection");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          TestHttpConnection http://localhost:43345");
            Console.WriteLine("          TestHttpConnection $BackendUrl$:$BackendPort$/");
            Console.WriteLine("          TestHttpConnection $WebAppUrl$:$WebAppPort$/");
            Wait();
        }
    }
}
