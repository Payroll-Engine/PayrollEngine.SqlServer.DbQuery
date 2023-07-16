using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal sealed class TestVersionCommand : CommandBase
    {
        public const int DefaultQueryTimeout = 5;

        internal async Task TestAsync(string tableName, string majorVersionColumnName,
            string minorVersionColumnName, string subVersionColumnName, string minVersion, string connectionString = null)
        {
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
            var dataSource = connectionInfo["Data Source"] as string;

            // user info
            WriteTitleLine("Test version");
            Console.WriteLine($"Server:       {dataSource}");
            Console.WriteLine();

            try
            {
                var available = await TestServerAvailableAsync(tableName, majorVersionColumnName,
                    minorVersionColumnName, subVersionColumnName, minVersion, connectionString);

                Console.WriteLine();
                if (available)
                {
                    WriteSuccessLine($"SQL Server {dataSource} is available.");
                }
                else
                {
                    Environment.ExitCode = -1;
                    WriteErrorLine($"SQL Server {dataSource} is not available.");
                }
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static async Task<bool> TestServerAvailableAsync(string tableName, string majorVersionColumnName,
            string minorVersionColumnName, string subVersionColumnName, string minVersion, string connectionString)
        {
            var testVersion = new Version(minVersion);
            try
            {
                var connection = new SqlConnection(connectionString);
                var query = $"SELECT {majorVersionColumnName}, {minorVersionColumnName}, {subVersionColumnName} FROM {tableName}";
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
            Console.WriteLine("      Output:");
            Console.WriteLine("          Exit code -1: version is not available");
            Console.WriteLine("      Examples:");
            Console.WriteLine("          TestVersion VersionServer ");
            Wait();
        }
    }
}
