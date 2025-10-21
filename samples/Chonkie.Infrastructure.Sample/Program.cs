using System;
using System.Threading.Tasks;
using Chonkie.Fetchers;
using Chonkie.Chefs;
using Chonkie.Refineries;
using Chonkie.Porters;
using Chonkie.Core.Types;
using System.Collections.Generic;

namespace Chonkie.Infrastructure.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Fetch text files from a directory
            var fetcher = new FileFetcher();
            var files = await fetcher.FetchAsync("./data", "*.txt");

            // Preprocess text
            var chef = new TextChef();
            var processed = new List<string>();
            foreach (var file in files)
                processed.Add(await chef.ProcessAsync(file.Content));

            // Create chunks (dummy example)
            var chunks = new List<Chunk>();
            foreach (var text in processed)
            {
                chunks.Add(new Chunk { Text = text, StartIndex = 0, EndIndex = text.Length, TokenCount = text.Split(' ').Length });
            }

            // Refine chunks (merge overlaps)
            var refinery = new OverlapRefinery(minOverlap: 8);
            var refinedChunks = await refinery.RefineAsync(chunks);

            // Export to JSON
            var porter = new JsonPorter();
            await porter.ExportAsync(refinedChunks, "output.json");

            Console.WriteLine($"Exported {refinedChunks.Count} chunks to output.json");
        }
    }
}
