using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PayrollEngine.SqlServer.DbQuery
{
    internal static class SharedConfiguration
    {
        /// <summary>Shared settings environment variable name, containing the json file name</summary>
        private static readonly string SharedConfigurationVariable = "PayrollConfiguration";

        internal static async Task<string> ParseAsync(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || !expression.Contains('$'))
            {
                return null;
            }

            var sharedConfiguration = await ReadAsync();
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

        internal static async Task<Dictionary<string, string>> ReadAsync()
        {
            var sharedConfigFileName = Environment.GetEnvironmentVariable(SharedConfigurationVariable);
            if (string.IsNullOrWhiteSpace(sharedConfigFileName) || !File.Exists(sharedConfigFileName))
            {
                return new();
            }
            try
            {
                var json = await File.ReadAllTextAsync(sharedConfigFileName);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            catch
            {
                return new();
            }
        }

        /// <summary>
        /// Get shared configuration value
        /// </summary>
        public static string GetSharedValue(Dictionary<string, string> sharedConfiguration, string name)
        {
            // primary name
            if (sharedConfiguration.TryGetValue(name, out var value))
            {
                return value;
            }

            // alternative name
            name = name.FirstCharacterToLower();
            if (sharedConfiguration.TryGetValue(name, out value))
            {
                return value;
            }

            return null;
        }

        // copy from Core StringExtensions
        private static string FirstCharacterToLower(this string value) =>
            char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
}