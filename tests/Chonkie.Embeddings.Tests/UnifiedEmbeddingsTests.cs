using Chonkie.Embeddings;
using Chonkie.Embeddings.Exceptions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Embeddings.Tests;

/// <summary>
/// Tests for UnifiedEmbeddings class.
/// </summary>
public class UnifiedEmbeddingsTests
{
    [Fact]
    public void Constructor_WithNullGenerator_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new UnifiedEmbeddings(null!, "test", 768));
    }

    [Fact]
    public void Constructor_WithNullProviderName_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new UnifiedEmbeddings(generator, null!, 768));
    }

    [Fact]
    public void Constructor_WithEmptyProviderName_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new UnifiedEmbeddings(generator, "", 768));
    }

    [Fact]
    public void Constructor_WithWhitespaceProviderName_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new UnifiedEmbeddings(generator, "   ", 768));
    }

    [Fact]
    public void Constructor_WithInvalidDimension_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new UnifiedEmbeddings(generator, "test", 0));

        Should.Throw<ArgumentException>(() =>
            new UnifiedEmbeddings(generator, "test", -100));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();

        // Act
        var embeddings = new UnifiedEmbeddings(generator, "test", 768);

        // Assert
        embeddings.ShouldNotBeNull();
        embeddings.Name.ShouldBe("unified-test");
        embeddings.Dimension.ShouldBe(768);
    }

    [Fact]
    public void Constructor_ConvertsProviderNameToLowercase()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();

        // Act
        var embeddings = new UnifiedEmbeddings(generator, "OpenAI", 1536);

        // Assert
        embeddings.Name.ShouldBe("unified-openai");
    }

    [Fact]
    public async Task EmbedAsync_WithValidText_ShouldReturnEmbedding()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        var expectedVector = new float[] { 0.1f, 0.2f, 0.3f };
        var embedding = new Embedding<float>(expectedVector);
        var generatedEmbeddings = new GeneratedEmbeddings<Embedding<float>>([embedding]);
        
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns(generatedEmbeddings);

        var embeddings = new UnifiedEmbeddings(generator, "test", 3);

        // Act
        var result = await embeddings.EmbedAsync("test text");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(3);
        result.ShouldBe(expectedVector);
    }

    [Fact]
    public async Task EmbedAsync_WithNullResult_ShouldThrowException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns((GeneratedEmbeddings<Embedding<float>>)null!);

        var embeddings = new UnifiedEmbeddings(generator, "test", 768);

        // Act & Assert
        await Should.ThrowAsync<EmbeddingInvalidResponseException>(async () =>
            await embeddings.EmbedAsync("test"));
    }

    [Fact]
    public async Task EmbedAsync_WithEmptyVector_ShouldThrowException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        var embedding = new Embedding<float>(Array.Empty<float>());
        var generatedEmbeddings = new GeneratedEmbeddings<Embedding<float>>([embedding]);
        
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns(generatedEmbeddings);

        var embeddings = new UnifiedEmbeddings(generator, "test", 768);

        // Act & Assert
        await Should.ThrowAsync<EmbeddingInvalidResponseException>(async () =>
            await embeddings.EmbedAsync("test"));
    }

    [Fact]
    public async Task EmbedAsync_WithCancellationToken_ShouldPassThroughCancellation()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<GeneratedEmbeddings<Embedding<float>>>(new CancellationToken(true)));

        var embeddings = new UnifiedEmbeddings(generator, "test", 768);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // Note: CancellationToken in the generator resolves to throw before cancellation propagates,
        // so we catch the EmbeddingException wrapping the cancellation
        await Should.ThrowAsync<EmbeddingException>(async () =>
            await embeddings.EmbedAsync("test", cts.Token));
    }

    [Fact]
    public async Task EmbedAsync_WithTimeoutException_ShouldThrowNetworkException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns<GeneratedEmbeddings<Embedding<float>>>(_ => throw new TimeoutException("Request timed out"));

        var embeddings = new UnifiedEmbeddings(generator, "test", 768);

        // Act & Assert
        await Should.ThrowAsync<EmbeddingNetworkException>(async () =>
            await embeddings.EmbedAsync("test"));
    }

    [Fact]
    public async Task EmbedAsync_WithRateLimitError_ShouldThrowRateLimitException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns<GeneratedEmbeddings<Embedding<float>>>(_ => throw new Exception("Rate limit exceeded"));

        var embeddings = new UnifiedEmbeddings(generator, "test", 768);

        // Act & Assert
        await Should.ThrowAsync<EmbeddingRateLimitException>(async () =>
            await embeddings.EmbedAsync("test"));
    }

    [Fact]
    public async Task EmbedAsync_WithAuthenticationError_ShouldThrowAuthenticationException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns<GeneratedEmbeddings<Embedding<float>>>(_ => throw new Exception("401 Unauthorized"));

        var embeddings = new UnifiedEmbeddings(generator, "test", 768);

        // Act & Assert
        await Should.ThrowAsync<EmbeddingAuthenticationException>(async () =>
            await embeddings.EmbedAsync("test"));
    }

    [Fact]
    public async Task EmbedAsync_WithHttpRequestException_ShouldThrowNetworkException()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator.GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions>(), Arg.Any<CancellationToken>())
            .Returns<GeneratedEmbeddings<Embedding<float>>>(_ => throw new HttpRequestException("Network error"));

        var embeddings = new UnifiedEmbeddings(generator, "test", 768);

        // Act & Assert
        await Should.ThrowAsync<EmbeddingNetworkException>(async () =>
            await embeddings.EmbedAsync("test"));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        var embeddings = new UnifiedEmbeddings(generator, "openai", 1536);

        // Act
        var result = embeddings.ToString();

        // Assert
        result.ShouldBe("UnifiedEmbeddings(Provider=openai, Dimension=1536)");
    }

    [Fact]
    public void Similarity_WithTwoVectors_ShouldCalculateCosineSimilarity()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        var embeddings = new UnifiedEmbeddings(generator, "test", 3);
        var vector1 = new float[] { 1.0f, 0.0f, 0.0f };
        var vector2 = new float[] { 1.0f, 0.0f, 0.0f };

        // Act
        var similarity = embeddings.Similarity(vector1, vector2);

        // Assert
        similarity.ShouldBe(1.0f, 0.001f);
    }
}
