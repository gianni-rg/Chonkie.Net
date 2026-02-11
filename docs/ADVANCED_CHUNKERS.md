# Advanced Chunkers - Optional Implementation Notes

## Overview

This document outlines the requirements and considerations for implementing additional advanced chunkers that are marked as optional in Phase 3 of the Chonkie.Net port.

## Implemented Chunkers

### ✅ SemanticChunker
**Status:** Complete  
**Location:** `src/Chonkie.Chunkers/SemanticChunker.cs`  
**Tests:** `tests/Chonkie.Core.Tests/Chunkers/SemanticChunkerTests.cs`

Uses similarity-based peak detection to find split points. Integrates with embedding providers to calculate semantic similarity between sentence windows.

**Key Features:**
- Configurable similarity threshold
- Window-based similarity calculation
- Skip-and-merge for non-consecutive groups
- Respects chunk size limits

### ✅ LateChunker
**Status:** Complete  
**Location:** `src/Chonkie.Chunkers/LateChunker.cs`  
**Tests:** `tests/Chonkie.Core.Tests/Chunkers/LateChunkerTests.cs`

Implements the "embed-then-chunk" approach. First chunks text using recursive splitting, then adds embeddings to each chunk.

**Key Features:**
- Extends RecursiveChunker
- Adds embeddings after chunking
- Supports custom recursive rules
- Maintains all RecursiveChunker functionality

## Optional Chunkers

### 🔄 CodeChunker (Future Enhancement)
**Status:** Not Implemented - Requires Additional Research  
**Priority:** Medium  
**Complexity:** High

#### Requirements

**Purpose:** Code-aware structural chunking that respects language syntax and structure.

**Python Implementation Uses:**
- `tree-sitter` for parsing
- Language-specific grammars
- AST-based splitting

**.NET Alternatives:**
1. **Roslyn** (.NET Compiler Platform)
   - Pros: Official Microsoft library, excellent C#/VB.NET support
   - Cons: Limited to .NET languages only
   - Package: `Microsoft.CodeAnalysis`

2. **Tree-sitter Bindings**
   - Pros: Same as Python version, multi-language support
   - Cons: No official .NET bindings, would need P/Invoke or community packages
   - Options:
     - `TreeSitterSharp` (community)
     - Direct P/Invoke to tree-sitter C library

3. **Language-Specific Parsers**
   - Pros: Optimized for specific languages
   - Cons: Would need separate implementation per language
   - Examples:
     - `Esprima.NET` for JavaScript
     - `RoslynPad.Roslyn` for C#
     - Custom parsers for other languages

#### Implementation Considerations

**Architecture:**
```csharp
public class CodeChunker : BaseChunker
{
    private readonly ICodeParser _parser;
    private readonly string _language;
    
    public CodeChunker(
        ITokenizer tokenizer,
        string language,
        ICodeParser? parser = null,
        int chunkSize = 2048)
        : base(tokenizer)
    {
        _language = language;
        _parser = parser ?? CodeParserFactory.Create(language);
        ChunkSize = chunkSize;
    }
    
    public override IReadOnlyList<Chunk> Chunk(string code)
    {
        // 1. Parse code into AST
        // 2. Identify structural boundaries (functions, classes, etc.)
        // 3. Split at appropriate points
        // 4. Respect chunk size limits
    }
}
```

**Challenges:**
- Multi-language support requires different parsers
- Tree-sitter .NET bindings are not mature
- Maintaining feature parity with Python version
- Performance implications of parsing

**Recommendation:** Implement when there's a clear user demand and mature .NET tree-sitter bindings become available. Start with Roslyn support for .NET languages as a proof of concept.

---

### 🔄 NeuralChunker (Future Enhancement)
**Status:** Not Implemented - Requires ONNX Integration  
**Priority:** Low-Medium  
**Complexity:** Medium-High

#### Requirements

**Purpose:** ML model-based chunking using neural networks.

**Python Implementation Uses:**
- Custom trained models
- `transformers` library
- PyTorch/TensorFlow

**.NET Implementation Path:**
- Use `Microsoft.ML.OnnxRuntime` for model execution
- Export Python models to ONNX format
- Load and run in .NET

**Architecture:**
```csharp
public class NeuralChunker : BaseChunker
{
    private readonly InferenceSession _session;
    private readonly string _modelPath;
    
    public NeuralChunker(
        ITokenizer tokenizer,
        string modelPath,
        int chunkSize = 2048)
        : base(tokenizer)
    {
        _modelPath = modelPath;
        _session = new InferenceSession(modelPath);
        ChunkSize = chunkSize;
    }
    
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        // 1. Prepare input tensors
        // 2. Run inference
        // 3. Process output to determine split points
        // 4. Create chunks
    }
}
```

**Dependencies:**
```xml
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.17.0" />
<!-- Optional for GPU support -->
<PackageReference Include="Microsoft.ML.OnnxRuntime.Gpu" Version="1.17.0" />
```

**Challenges:**
- Need trained models in ONNX format
- Model distribution strategy
- Performance optimization
- GPU acceleration setup

**Recommendation:** Implement when specific use cases emerge that require ML-based chunking and ONNX models are available.

---

### 🔄 SlumberChunker (Future Enhancement)
**Status:** Not Implemented - Requires LLM Integration  
**Priority:** Low  
**Complexity:** Medium

#### Requirements

**Purpose:** LLM-guided agentic chunking using large language models.

**Python Implementation Uses:**
- LLM API calls (OpenAI, etc.)
- Prompt engineering
- Iterative refinement

**.NET Implementation Path:**
- Reuse existing `IGenie` interface from Chonkie.Genies
- Integrate with OpenAI, Azure OpenAI, or Gemini
- Implement prompt-based chunking logic

**Architecture:**
```csharp
public class SlumberChunker : BaseChunker
{
    private readonly IGenie _genie;
    private readonly string _prompt Template;
    
    public SlumberChunker(
        ITokenizer tokenizer,
        IGenie genie,
        int chunkSize = 2048,
        string? promptTemplate = null)
        : base(tokenizer)
    {
        _genie = genie;
        _promptTemplate = promptTemplate ?? DefaultPromptTemplate;
        ChunkSize = chunkSize;
    }
    
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        // 1. Build prompt with text and instructions
        // 2. Call LLM via IGenie
        // 3. Parse LLM response for split points
        // 4. Create chunks based on LLM guidance
    }
}
```

**Dependencies:**
- Phase 8 completion (LLM Genies)
- `Chonkie.Genies` project

**Challenges:**
- API costs for each chunking operation
- Latency from LLM calls
- Prompt engineering for reliable results
- Error handling for API failures
- Rate limiting

**Recommendation:** Implement after Phase 8 completion when Genie infrastructure is ready. Best suited for specific use cases where semantic understanding is critical.

---

### ✅ TableChunker (Documented - Not a Priority)
**Status:** Lower Priority - Specialized Use Case  
**Complexity:** Medium

**Purpose:** Table-aware chunking that preserves table structure.

**Implementation Notes:**
- Could extend MarkdownChef/TableChef functionality
- Detect table boundaries
- Keep tables intact or split intelligently
- Handle various table formats (Markdown, HTML, CSV-in-text)

**Recommendation:** Implement when table-specific use cases arise. May fit better as part of Chef preprocessing than as a separate chunker.

---

## Implementation Priority

Based on user demand and complexity:

1. **High Priority (Phase 3):**
   - ✅ SemanticChunker - COMPLETE
   - ✅ LateChunker - COMPLETE

2. **Medium Priority (Future Phases):**
   - 🔄 CodeChunker - When tree-sitter bindings mature
   - 🔄 NeuralChunker - When ONNX models available

3. **Low Priority (On Demand):**
   - 🔄 SlumberChunker - After Phase 8, for specific use cases
   - 🔄 TableChunker - Specialized scenarios

## Conclusion

Phase 3 core objectives have been achieved with SemanticChunker and LateChunker implementations. Optional chunkers are documented for future implementation based on user demand and technical readiness.

**Current Test Coverage:**
- SemanticChunker: 7 tests
- LateChunker: 6 tests
- Total Phase 3 tests: 13 new tests
- Overall project: 239 tests, 100% passing

**Next Steps:**
- Proceed to Phase 4 (Supporting Infrastructure)
- Revisit optional chunkers based on community feedback
- Monitor for mature .NET alternatives to Python dependencies
