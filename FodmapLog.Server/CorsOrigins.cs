using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace FodmapLog.Server
{
    /// <summary>
    /// Allowed browser origins for the SPA. The API is the same App Service;
    /// a custom domain still needs CORS while cached clients call azurewebsites.net.
    /// Extra hosts: Cors__Origins__0 in App Settings.
    /// </summary>
    internal static class CorsOrigins
    {
        internal static readonly string[] BuiltIn =
        [
            "https://localhost:4200",
            "http://localhost:4200",
            "https://127.0.0.1:4200",
            "http://127.0.0.1:4200",
            "https://healthygultlog-app.azurewebsites.net",
            "https://mova.app.pl",
            "https://www.mova.app.pl"
        ];

        internal static string[] Combine(IConfiguration configuration)
        {
            var extra = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
            return BuiltIn
                .Concat(extra)
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.Trim().TrimEnd('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
