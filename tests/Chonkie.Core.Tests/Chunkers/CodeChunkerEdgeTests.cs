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

namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Xunit;

public class CodeChunkerEdgeTests
{
    [Fact]
    public void Chunk_EmptyInput_ReturnsEmpty()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer);

        var chunks = chunker.Chunk("");

        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_WhitespaceInput_ReturnsEmpty()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer);

        var chunks = chunker.Chunk("   \t\n  ");

        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_JavaScriptCode_ChunksCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 30);

        var jsCode = @"
function greet(name) {
  console.log(`Hello, ${name}!`);
}

class Calculator {
  add(a, b) {
    return a + b;
  }
}

const calc = new Calculator();
greet('Developer');
console.log(calc.add(5, 3));
";

        var chunks = chunker.Chunk(jsCode);

        Assert.NotEmpty(chunks);
        Assert.True(chunks.Count > 0);
        Assert.All(chunks, c => Assert.NotNull(c.Text));
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(jsCode.Length, chunks[^1].EndIndex);

        // Verify reconstruction
        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        Assert.Equal(jsCode, reconstructed);
    }

    [Fact]
    public void Chunk_ReconstructionPreservesOriginalText()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 50);

        var code = @"import os
import sys

def hello_world(name: str):
    print(f""Hello, {name}!"")

class MyClass:
    def __init__(self, value):
        self.value = value

    def get_value(self):
        return self.value

if __name__ == ""__main__"":
    hello_world(""World"")
    instance = MyClass(10)
    print(instance.get_value())
";

        var chunks = chunker.Chunk(code);

        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        Assert.Equal(code, reconstructed);
    }

    [Fact]
    public void Chunk_AdheresToChunkSize()
    {
        var tokenizer = new CharacterTokenizer();
        var chunkSize = 50;
        var chunker = new CodeChunker(tokenizer, chunkSize: chunkSize);

        var code = @"import os
import sys

def hello_world(name: str):
    print(f""Hello, {name}!"")

class MyClass:
    def __init__(self, value):
        self.value = value
";

        var chunks = chunker.Chunk(code);

        // Allow some leeway as splitting happens at node boundaries
        for (int i = 0; i < chunks.Count - 1; i++)
        {
            Assert.True(chunks[i].TokenCount < chunkSize + 20);
        }
        Assert.True(chunks[^1].TokenCount > 0);
    }

    [Fact]
    public void Chunk_IndicesAreSequential()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 50);

        var code = @"import os
import sys

def hello_world(name: str):
    print(f""Hello, {name}!"")

class MyClass:
    def __init__(self, value):
        self.value = value
";

        var chunks = chunker.Chunk(code);
        var currentIndex = 0;

        foreach (var chunk in chunks)
        {
            Assert.Equal(currentIndex, chunk.StartIndex);
            Assert.Equal(currentIndex + chunk.Text.Length, chunk.EndIndex);
            Assert.Equal(chunk.Text, code.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex));
            currentIndex = chunk.EndIndex;
        }

        Assert.Equal(code.Length, currentIndex);
    }

    [Fact]
    public void Chunk_ReturnsChunkObjects()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 50);

        var code = @"def foo():
    return 42

def bar():
    return 'hello'
";

        var chunks = chunker.Chunk(code);

        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.NotNull(chunk);
            Assert.NotNull(chunk.Text);
            Assert.True(chunk.StartIndex >= 0);
            Assert.True(chunk.EndIndex > chunk.StartIndex);
            Assert.True(chunk.TokenCount > 0);
        });

        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        Assert.Equal(code, reconstructed);
    }
}
