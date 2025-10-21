using System.Threading.Tasks;
using Xunit;
using Chonkie.Chefs;

namespace Chonkie.Chefs.Tests
{
    public class TextChefTests
    {
        [Fact]
        public async Task ProcessAsync_TrimsAndNormalizesWhitespace()
        {
            var chef = new TextChef();
            var input = "  Hello\t\nWorld!  ";
            var result = await chef.ProcessAsync(input);
            Assert.Equal("Hello World!", result);
        }
    }
}
