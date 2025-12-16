using System;
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

        /// <summary>
        /// Processes code text span directly without allocations.
        /// C# 14 implicit span conversion allows passing strings directly.
        /// </summary>
        /// <param name="text">The code text span to process.</param>
        /// <returns>The text as a string (pass-through for code).</returns>
        public string Process(ReadOnlySpan<char> text)
        {
            // Code chef is pass-through, but we must convert to string
            return text.ToString();
        }
    }
}
