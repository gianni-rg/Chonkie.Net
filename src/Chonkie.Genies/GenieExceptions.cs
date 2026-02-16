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

namespace Chonkie.Genies;

/// <summary>
/// Base exception for all Genie-related errors.
/// </summary>
public class GenieException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenieException"/> class.
    /// </summary>
    public GenieException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenieException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public GenieException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenieException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public GenieException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when rate limits are exceeded.
/// </summary>
public class RateLimitException : GenieException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    public RateLimitException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RateLimitException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RateLimitException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when authentication fails.
/// </summary>
public class AuthenticationException : GenieException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
    /// </summary>
    public AuthenticationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AuthenticationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public AuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when JSON parsing or validation fails.
/// </summary>
public class JsonParsingException : GenieException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonParsingException"/> class.
    /// </summary>
    public JsonParsingException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonParsingException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public JsonParsingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonParsingException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public JsonParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
