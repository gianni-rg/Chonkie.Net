using System;

namespace Xunit;

/// <summary>
/// Provides a v3-compatible attribute name for existing SkippableFact tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class SkippableFactAttribute : FactAttribute
{
    public SkippableFactAttribute(string? sourceInformation = null)
    {
    }
}

/// <summary>
/// Exception used to indicate a skipped test.
/// </summary>
public sealed class SkipException : Exception
{
    public SkipException(string? message = null)
        : base(message ?? "Test skipped.")
    {
    }
}

/// <summary>
/// Provides conditional skip helpers compatible with xUnit v3.
/// </summary>
public static class Skip
{
    public static void If(bool condition, string? reason = null)
    {
        if (condition)
        {
            throw new SkipException(reason ?? "Test skipped.");
        }
    }
}
