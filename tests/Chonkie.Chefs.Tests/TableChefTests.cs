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

using System.Threading.Tasks;
using Xunit;
using Chonkie.Chefs;

namespace Chonkie.Chefs.Tests
{
    public class TableChefTests
    {
        [Fact]
        public async Task ProcessAsync_ExtractsMarkdownTable()
        {
            var chef = new TableChef();
            var input = "| Col1 | Col2 |\n|------|------|\n| A    | B    |\n";
            var result = await chef.ProcessAsync(input);
            Assert.Contains("| Col1 | Col2 |", result);
            Assert.Contains("| A    | B    |", result);
        }
    }
}
