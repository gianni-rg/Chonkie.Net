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
