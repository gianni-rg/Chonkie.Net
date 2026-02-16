// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
