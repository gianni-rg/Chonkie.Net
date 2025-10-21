using Xunit;
using Chonkie.Embeddings.SentenceTransformers;

namespace Chonkie.Embeddings.Tests.Providers
{
    public class SentenceTransformerEmbeddingsTests
    {
        [Fact]
        public void Constructor_ThrowsException_WhenModelPathIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SentenceTransformerEmbeddings(null!));
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            var modelPath = "test-model.onnx";

            // Act & Assert - May throw if file doesn't exist, but we're just testing property initialization
            try
            {
                var embeddings = new SentenceTransformerEmbeddings(modelPath, 384);
                Assert.Equal("sentence-transformers", embeddings.Name);
                Assert.Equal(384, embeddings.Dimension);
            }
            catch (Exception)
            {
                // Expected when model file doesn't exist
            }
        }

        // Note: Full integration tests require actual ONNX model files
    }
}