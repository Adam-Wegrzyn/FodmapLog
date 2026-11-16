using Microsoft.Data.SqlClient;

namespace FodmapLog.Server
{
    /// <summary>
    /// Azure SQL serverless can take over 30s to resume. Default connect timeout
    /// then fails Google login on the first request after idle.
    /// </summary>
    internal static class SqlConnectionOptions
    {
        internal const int MinConnectTimeoutSeconds = 90;

        internal static string WithMinimumConnectTimeout(string? connectionString, int seconds = MinConnectTimeoutSeconds)
        {
            var builder = new SqlConnectionStringBuilder(connectionString ?? string.Empty);
            if (builder.ConnectTimeout < seconds)
            {
                builder.ConnectTimeout = seconds;
            }
            return builder.ConnectionString;
        }
    }
}
