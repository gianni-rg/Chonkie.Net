using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Fetchers
{
    /// <summary>
    /// Fetches text files from a file or directory, with optional filtering.
    /// </summary>
    public class FileFetcher : IFetcher
    {
        public async Task<IReadOnlyList<(string Path, string Content)>> FetchAsync(string path, string? filter = null, CancellationToken cancellationToken = default)
        {
            var results = new List<(string Path, string Content)>();
            IEnumerable<string> files;

            if (Directory.Exists(path))
            {
                files = Directory.EnumerateFiles(path, filter ?? "*.*", SearchOption.AllDirectories);
            }
            else if (File.Exists(path))
            {
                files = new[] { path };
            }
            else
            {
                throw new FileNotFoundException($"Path not found: {path}");
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                results.Add((file, content));
            }

            return results;
        }
    }
}
