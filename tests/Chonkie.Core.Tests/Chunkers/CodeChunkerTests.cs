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

public class CodeChunkerTests
{
    [Fact]
    public void Chunk_SplitsAtFunctionBoundaries_WhenExceedingBudget()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 60);

        var code = @"public class Demo {
public void A() {
    var x = 1;
    var y = 2;
}

public void B() {
    for (int i=0;i<10;i++) {
        System.Console.WriteLine(i);
    }
}
}";

        var chunks = chunker.Chunk(code);

        Assert.NotEmpty(chunks);
        Assert.True(chunks.Count >= 2);
        // Ensure chunks cover the entire text
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);
        // Token counts should be within budget
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 60));
    }

    [Fact]
    public void Chunk_PreservesExactStringIndices()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 50);

        var code = @"class A {
void M1() {}
void M2() {}
void M3() {}
}";

        var chunks = chunker.Chunk(code);

        // Verify complete coverage
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);

        // Verify each chunk text matches original at indices
        foreach (var chunk in chunks)
        {
            var extracted = code.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            Assert.Equal(chunk.Text, extracted);
        }
    }

    [Fact]
    public void Chunk_MultipleFunctions_SplitsCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 40);

        var code = @"public void A() { return 1; }
public void B() { return 2; }
public void C() { return 3; }
public void D() { return 4; }";

        var chunks = chunker.Chunk(code);

        Assert.True(chunks.Count >= 2);
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 40));
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_WithComments_HandlesCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 50);

        var code = @"// Comment line 1
// Comment line 2
public void Method() {
    // inline comment
    var x = 1;
}";

        var chunks = chunker.Chunk(code);

        Assert.NotEmpty(chunks);
        // Verify exact coverage
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_MixedStructures_SplitsAtBoundaries()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 60);

        var code = @"namespace N {
class C {
    public int Prop { get; set; }

    public void Method() {
        if (true) {
            var x = 1;
        }
    }
}
interface I {
    void DoWork();
}
}";

        var chunks = chunker.Chunk(code);

        Assert.NotEmpty(chunks);
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 60));
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_VerySmallBudget_StillProducesValidChunks()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 10);

        var code = @"public class Test { public void M() { var a = 1; } }";

        var chunks = chunker.Chunk(code);

        Assert.NotEmpty(chunks);
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 10));
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_SingleLineFunctions_GroupsAppropriately()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 100);

        var code = @"void A() { }
void B() { }
void C() { }
void D() { }
void E() { }";

        var chunks = chunker.Chunk(code);

        Assert.NotEmpty(chunks);
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);

        // Verify no gaps or overlaps
        for (int i = 1; i < chunks.Count; i++)
        {
            Assert.Equal(chunks[i - 1].EndIndex, chunks[i].StartIndex);
        }
    }

    [Fact]
    public void Chunk_NestedBlocks_HandlesCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new CodeChunker(tokenizer, chunkSize: 80);

        var code = @"public void Outer() {
    if (condition) {
        while (true) {
            for (int i = 0; i < 10; i++) {
                Console.WriteLine(i);
            }
        }
    }
}";

        var chunks = chunker.Chunk(code);

        Assert.NotEmpty(chunks);
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 80));
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(code.Length, chunks[^1].EndIndex);
    }
}
