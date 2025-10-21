using System.IO;
using System.Threading.Tasks;
using Xunit;
using Chonkie.Fetchers;

namespace Chonkie.Fetchers.Tests
{
    public class FileFetcherTests
    {
        [Fact]
        public async Task FetchAsync_ReturnsFileContent()
        {
            // Arrange
            var fetcher = new FileFetcher();
            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "hello world");

            // Act
            var result = await fetcher.FetchAsync(tempFile);

            // Assert
            Assert.Single(result);
            Assert.Equal(tempFile, result[0].Path);
            Assert.Equal("hello world", result[0].Content);

            // Cleanup
            File.Delete(tempFile);
        }
    }
}
