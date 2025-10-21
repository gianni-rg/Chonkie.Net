using System.Threading.Tasks;
using Xunit;
using Chonkie.Chefs;
using Markdig;

namespace Chonkie.Chefs.Tests
{
    public class MarkdownChefTests
    {
        [Fact]
        public async Task ProcessAsync_ConvertsMarkdownToHtml()
        {
            var chef = new MarkdownChef();
            var input = "# Title\n\nSome text.";
            var result = await chef.ProcessAsync(input);
            Assert.Contains("<h1>Title</h1>", result);
            Assert.Contains("<p>Some text.</p>", result);
        }
    }
}
