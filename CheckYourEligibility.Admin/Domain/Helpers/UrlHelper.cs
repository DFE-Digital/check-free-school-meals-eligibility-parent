using System.Web;

namespace CheckYourEligibility.Admin.Domain.Helpers;

public static class UrlHelper
{

    /// <summary>
    /// Removes a specific occurrence of a query string parameter with a given value.
    /// </summary>
    /// <param name="url">The original URL.</param>
    /// <param name="key">The query string key to target.</param>
    /// <param name="value">The value to match for removal.</param>
    /// <returns>Updated URL with the specified occurrence removed.</returns>
    public static string RemoveQueryParamOccurrence(string url, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key)) { return url; }

        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Query);

        // Convert to list to handle duplicates
        var values = query.GetValues(key)?.ToList();
        if (values == null) { return url; }

        // Match by value 
        for (int i = 0; i < values.Count; i++)
        {
            if (string.Equals(values[i], value, StringComparison.OrdinalIgnoreCase))
            {
                values.RemoveAt(i);
                break;
            }
        }

        // Clear and re-add updated values
        query.Remove(key);
        if (values.Count > 0)
        {
            foreach (var v in values)
                query.Add(key, v);
        }

        // Rebuild URL
        var uriBuilder = new UriBuilder(uri)
        {
            Query = query.ToString()
        };
        return uriBuilder.ToString();
    }

}