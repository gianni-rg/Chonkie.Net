using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

namespace Chonkie.Porters
{
    /// <summary>
    /// Exports chunked data to a JSON file.
    /// </summary>
    public class JsonPorter : IPorter
    {
        public async Task<bool> ExportAsync(IReadOnlyList<Chunk> chunks, string destination, CancellationToken cancellationToken = default)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            using var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, chunks, options, cancellationToken);
            return true;
        }
    }
}
