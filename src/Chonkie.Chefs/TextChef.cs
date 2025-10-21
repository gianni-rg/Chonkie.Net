using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Basic text preprocessing: trims, normalizes whitespace, removes control characters.
    /// </summary>
    public class TextChef : IChef
    {
        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(string.Empty);

            // Remove control characters
            var cleaned = Regex.Replace(text, "[\x00-\x1F\x7F]", "");
            // Normalize whitespace
            cleaned = Regex.Replace(cleaned, "\\s+", " ").Trim();
            return Task.FromResult(cleaned);
        }
    }
}
