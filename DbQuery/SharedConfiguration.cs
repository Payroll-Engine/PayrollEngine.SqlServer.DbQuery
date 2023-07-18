using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal static class SharedConfiguration
    {
        /// <summary>Shared settings environment variable name, containing the json file name</summary>
        private static readonly string SharedConfigurationVariable = "PayrollConfiguration";

        internal static string Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || !expression.Contains('$'))
            {
                return null;
            }

            var sharedConfiguration = SharedConfiguration.GetSharedConfiguration();
            foreach (var config in sharedConfiguration)
            {
                var variable = $"${config.Key}$";
                if (expression.Contains(variable))
                {
                    expression = expression.Replace(variable, config.Value);
                }
            }
            return expression;
        }

        internal static Dictionary<string, string> GetSharedConfiguration()
        {
            var sharedConfigFileName = Environment.GetEnvironmentVariable(SharedConfigurationVariable);
            if (string.IsNullOrWhiteSpace(sharedConfigFileName) || !File.Exists(sharedConfigFileName))
            {
                return new();
            }
            try
            {
                var json = File.ReadAllText(sharedConfigFileName);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            catch
            {
                return new();
            }
        }
    }
}