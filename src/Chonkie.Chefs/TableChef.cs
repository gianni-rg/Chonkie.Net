using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Extracts and processes tables from text (basic implementation).
    /// </summary>
    public class TableChef : IChef
    {
        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(string.Empty);

            // Simple markdown table extraction (first table only)
            var match = Regex.Match(text, @"((?:\|.+\|\r?\n)+)");
            if (!match.Success)
                return Task.FromResult(string.Empty);

            var tableText = match.Groups[1].Value;
            // Optionally, parse to DataTable or CSV
            return Task.FromResult(tableText);
        }
    }
}
