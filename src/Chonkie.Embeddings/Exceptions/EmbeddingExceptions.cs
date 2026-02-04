namespace Chonkie.Embeddings.Exceptions;

/// <summary>
/// Base exception for all embedding-related errors.
/// </summary>
public class EmbeddingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EmbeddingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EmbeddingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingException"/> class.
    /// </summary>
    public EmbeddingException()
    {
    }
}

/// <summary>
/// Exception thrown when rate limiting occurs during embedding operations.
/// </summary>
public class EmbeddingRateLimitException : EmbeddingException
{
    /// <summary>
    /// Gets the retry-after time in seconds, if available.
    /// </summary>
    public int? RetryAfterSeconds { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingRateLimitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="retryAfterSeconds">Optional retry-after time in seconds.</param>
    public EmbeddingRateLimitException(string message, int? retryAfterSeconds = null) : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingRateLimitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="retryAfterSeconds">Optional retry-after time in seconds.</param>
    public EmbeddingRateLimitException(string message, Exception innerException, int? retryAfterSeconds = null) 
        : base(message, innerException)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingRateLimitException"/> class.
    /// </summary>
    public EmbeddingRateLimitException()
    {
    }
}

/// <summary>
/// Exception thrown when authentication fails during embedding operations.
/// </summary>
public class EmbeddingAuthenticationException : EmbeddingException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EmbeddingAuthenticationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EmbeddingAuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingAuthenticationException"/> class.
    /// </summary>
    public EmbeddingAuthenticationException()
    {
    }
}

/// <summary>
/// Exception thrown when a network failure occurs during embedding operations.
/// </summary>
public class EmbeddingNetworkException : EmbeddingException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingNetworkException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EmbeddingNetworkException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingNetworkException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EmbeddingNetworkException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingNetworkException"/> class.
    /// </summary>
    public EmbeddingNetworkException()
    {
    }
}

/// <summary>
/// Exception thrown when the embedding API returns an invalid response.
/// </summary>
public class EmbeddingInvalidResponseException : EmbeddingException
{
    /// <summary>
    /// Gets the HTTP status code, if available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingInvalidResponseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">Optional HTTP status code.</param>
    public EmbeddingInvalidResponseException(string message, int? statusCode = null) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingInvalidResponseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="statusCode">Optional HTTP status code.</param>
    public EmbeddingInvalidResponseException(string message, Exception innerException, int? statusCode = null) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingInvalidResponseException"/> class.
    /// </summary>
    public EmbeddingInvalidResponseException()
    {
    }
}
