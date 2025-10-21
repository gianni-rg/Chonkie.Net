using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Chonkie.Refineries;
using Chonkie.Core.Types;

namespace Chonkie.Refineries.Tests
{
    public class OverlapRefineryTests
    {
        [Fact]
        public async Task RefineAsync_MergesOverlappingChunks()
        {
            var chunks = new List<Chunk>
            {
                new Chunk { Text = "Hello ", StartIndex = 0, EndIndex = 6, TokenCount = 1 },
                new Chunk { Text = "World!", StartIndex = 6, EndIndex = 12, TokenCount = 1 }
            };
            var refinery = new OverlapRefinery(minOverlap: 8);
            var result = await refinery.RefineAsync(chunks);
            Assert.Single(result);
            Assert.Equal("Hello World!", result[0].Text);
        }
    }
}
