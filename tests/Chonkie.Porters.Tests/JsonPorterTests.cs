using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Chonkie.Porters;
using Chonkie.Core.Types;

namespace Chonkie.Porters.Tests
{
    public class JsonPorterTests
    {
        [Fact]
        public async Task ExportAsync_WritesJsonFile()
        {
            var porter = new JsonPorter();
            var tempFile = Path.GetTempFileName();
            var chunks = new List<Chunk>
            {
                new Chunk { Text = "Hello", StartIndex = 0, EndIndex = 5, TokenCount = 1 },
                new Chunk { Text = "World", StartIndex = 6, EndIndex = 11, TokenCount = 1 }
            };
            var success = await porter.ExportAsync(chunks, tempFile);
            Assert.True(success);
            var json = await File.ReadAllTextAsync(tempFile);
            Assert.Contains("Hello", json);
            Assert.Contains("World", json);
            File.Delete(tempFile);
        }
    }
}
