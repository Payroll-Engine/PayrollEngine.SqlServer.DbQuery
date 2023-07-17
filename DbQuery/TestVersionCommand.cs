using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestVersionCommand : CommandBase
    {
        public const int DefaultQueryTimeout = 5;

        internal async Task TestAsync(bool verbose, TestVersionSettings settings, string connectionString = null)
        {
            // connection string
            connectionString = ConnectionConfiguration.GetConnectionString(connectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Environment.ExitCode = -2;
                if (verbose)
                {
                    Console.WriteLine($"Missing database connection string {ConnectionConfiguration.DatabaseConnectionString}");
                    Wait();
                }
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
                Console.WriteLine();
            }

            try
            {
                var available = await TestServerAvailableAsync(settings, connectionString);

                // server not available
                if (!available)
                {
                    Environment.ExitCode = -1;
                    if (verbose)
                    {
                        Console.WriteLine();
                        WriteErrorLine($"SQL Server {dataSource} is not available.");
                        Console.WriteLine();
                    }
                    return;
                }

                // server available
                if (verbose)
                {
                    Console.WriteLine();
                    WriteSuccessLine($"SQL Server {dataSource} is available.");
                    Console.WriteLine();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static async Task<bool> TestServerAvailableAsync(TestVersionSettings settings, string connectionString)
        {
            var testVersion = new Version(settings.MinVersion);
            try
            {
                var connection = new SqlConnection(connectionString);
                var query = $"SELECT {settings.MajorVersionColumnName}, {settings.MinorVersionColumnName}, {settings.SubVersionColumnName} FROM {settings.TableName}";
                await using (connection)
                {
                    await using var command = new SqlCommand(query, connection);
                    await connection.OpenAsync();

                    var validVersion = false;
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var major = reader.GetInt32(0);
                        var minor = reader.GetInt32(1);
                        var sub = reader.GetInt32(2);

                        var curVersion = new Version(major, minor, sub);
                        if (curVersion >= testVersion)
                        {
                            validVersion = true;
                            break;
                        }
                    }
                    connection.Close();

                    return validVersion;
                }
            }
            catch
            {
                return false;
            }
        }

        internal static void ShowHelp()
        {
            WriteTitleLine("- TestVersion");
            Console.WriteLine("      Test SQL database server");
            Console.WriteLine("      Arguments:");
            Console.WriteLine("          1. The table name");
            Console.WriteLine("          2. The major version column name");
            Console.WriteLine("          3. The minor version column name");
            Console.WriteLine("          4. The subversion column name");
            Console.WriteLine("          5. Minimum required version");
            Console.WriteLine("          6. Database connection string (optional, default from app-settings)");
            Console.WriteLine("      Toggles:");
            Console.WriteLine("          verbose mode: /verbose (default: off)");
            Console.WriteLine("      Output:");
            Console.WriteLine("          Exit code -1: version is not available");
            Console.WriteLine("          Exit code -2: invalid database connection string");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          TestVersion VersionServer ");
            Wait();
        }
    }
}
