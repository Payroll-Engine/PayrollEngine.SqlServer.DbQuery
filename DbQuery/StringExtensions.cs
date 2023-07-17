using System;
using System.Linq;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal static class StringExtensions
    {
        /// <remarks>source: https://stackoverflow.com/a/49147314</remarks>
        internal static string SetConnectionTimeout(this string connectionString, int timeout)
        {
            var index = connectionString.IndexOf("Connection Timeout=", StringComparison.InvariantCultureIgnoreCase);
            if (index <= 0)
            {
                return connectionString;
            }

            var oldTimeoutString = new string(connectionString.Substring(index).Split('=')[1].Where(char.IsDigit).ToArray());
            if (!int.TryParse(oldTimeoutString, out var oldTimeout))
            {
                return connectionString;
            }
            return oldTimeout == timeout ?
                connectionString :
                connectionString.Replace($"Connection Timeout={oldTimeout}", $"Connection Timeout={timeout}");
        }
    }
}
