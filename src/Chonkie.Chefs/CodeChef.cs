using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Code preprocessing that preserves exact whitespace, newlines, and formatting.
    /// Unlike TextChef, this does not normalize whitespace or remove newlines.
    /// </summary>
    public class CodeChef : IChef
    {
        /// <inheritdoc />
        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            // Pass through unchanged - preserve exact formatting for code
            return Task.FromResult(text ?? string.Empty);
        }
    }
}
