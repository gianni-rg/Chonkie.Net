using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Defines the contract for text preprocessing components (chefs).
    /// </summary>
    public interface IChef
    {
        /// <summary>
        /// Preprocesses input text and returns cleaned/processed output.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Processed text.</returns>
        Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default);
    }
}
