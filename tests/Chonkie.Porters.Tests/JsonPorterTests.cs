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
