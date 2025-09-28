using System;
using Newtonsoft.Json.Linq;

namespace InstagramAuto.Client.Helpers
{
    public static class ErrorHelper
    {
        // Parse exception message that may include 'Details: {json}' and return user-friendly message and details
        public static (string Message, string Details) Parse(Exception ex)
        {
            if (ex == null) return (string.Empty, string.Empty);
            var msg = ex.Message ?? string.Empty;
            var details = string.Empty;
            var marker = "Details:";
            var idx = msg.IndexOf(marker, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var user = msg.Substring(0, idx).Trim();
                details = msg.Substring(idx + marker.Length).Trim();
                // try to pretty-print JSON
                try
                {
                    var parsed = JToken.Parse(details);
                    details = parsed.ToString(Newtonsoft.Json.Formatting.Indented);
                }
                catch { }
                return (user, details);
            }
            return (msg, string.Empty);
        }
    }
}
