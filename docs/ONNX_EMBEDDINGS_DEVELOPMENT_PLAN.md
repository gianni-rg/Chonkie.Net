# ONNX-Based Sentence Transformer Embeddings - Development Plan

**Date:** October 24, 2025
**Status:** 🟦 IN PROGRESS - Missing Components Identified

## Executive Summary

The ONNX-based Sentence Transformer embeddings implementation exists but has **critical missing components** that prevent it from working in production. This document outlines what's missing and what needs to be developed.

## Current Status

### ✅ What's Already Implemented

1. **Basic Structure** (`SentenceTransformerEmbeddings.cs`)
   - ✅ ONNX Runtime integration (v1.20.1)
   - ✅ InferenceSession initialization
   - ✅ Basic embedding generation interface
   - ✅ IDisposable pattern for resource cleanup
   - ✅ Batch processing support
   - ✅ Integration with AutoEmbeddings factory

2. **Testing Infrastructure**
   - ✅ Unit tests (basic validation)
   - ✅ Integration tests (35 tests total, 5 for Sentence Transformers)
   - ✅ Skip mechanism for missing model files
   - ✅ Test helpers and utilities

3. **Documentation**
   - ✅ README with usage examples
   - ✅ Integration test setup guide
   - ✅ Environment variable configuration

## ❌ Critical Missing Components

### 1. **PROPER TOKENIZER** (HIGHEST PRIORITY)

**Current Problem:**
```csharp
// From SentenceTransformerEmbeddings.cs:96-100
private long[] SimpleTokenize(string text)
{
    // This is a simplified tokenizer
    // In production, use Microsoft.ML.Tokenizers or similar
    var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return words.Select(w => (long)w.GetHashCode() % 30000).ToArray();
}
```

**This is a PLACEHOLDER** that:
- ❌ Does NOT use proper subword tokenization
- ❌ Will produce INCORRECT embeddings
- ❌ Is NOT compatible with Sentence Transformer models
- ❌ Cannot handle special tokens, padding, or truncation

**What's Needed:**
- [ ] Integration with `Microsoft.ML.Tokenizers` (v0.21.0+)
- [ ] Support for HuggingFace tokenizers (WordPiece, BPE, Unigram)
- [ ] Proper tokenizer configuration loading
- [ ] Special token handling ([CLS], [SEP], [PAD])
- [ ] Attention mask generation
- [ ] Token ID mapping compatible with ONNX model

**Reference Model:**
- Sentence Transformers models (e.g., `all-MiniLM-L6-v2`) use WordPiece tokenization
- Requires matching tokenizer vocabulary from the model

### 2. **ONNX MODEL ACQUISITION & MANAGEMENT**

**Current Problem:**
- ⚠️ No documented source for ONNX models
- ⚠️ No model download/caching mechanism
- ⚠️ Users must manually obtain and configure models
- ⚠️ No model validation or compatibility checks

**What's Needed:**

#### A. Model Conversion Pipeline
```bash
# Python script to convert HuggingFace models to ONNX
pip install optimum[onnxruntime]
optimum-cli export onnx --model sentence-transformers/all-MiniLM-L6-v2 ./models/all-MiniLM-L6-v2
```

#### B. Model Management Features
- [ ] Model download utility (from HuggingFace Hub)
- [ ] Model caching (local cache directory)
- [ ] Model validation (check input/output signatures)
- [ ] Model metadata loading (dimension, max_seq_length)
- [ ] Vocabulary/tokenizer loading alongside model

#### C. Documentation
- [ ] Guide for converting HuggingFace models to ONNX
- [ ] List of tested/supported models
- [ ] Model compatibility matrix
- [ ] Performance benchmarks per model

### 3. **TENSOR INPUT/OUTPUT HANDLING**

**Current Problem:**
```csharp
// From SentenceTransformerEmbeddings.cs:51-66
var inputIds = new DenseTensor<long>(new[] { 1, tokens.Length });
var attentionMask = new DenseTensor<long>(new[] { 1, tokens.Length });

// Simplified - doesn't handle:
// - Token type IDs (segment IDs)
// - Position IDs
// - Dynamic sequence lengths
// - Padding/truncation
// - Batch dimension properly
```

**What's Needed:**
- [ ] Proper input tensor construction
  - `input_ids`: Token IDs from proper tokenizer
  - `attention_mask`: Valid attention mask (1 for real tokens, 0 for padding)
  - `token_type_ids` (if required by model)
- [ ] Output tensor parsing
  - Extract pooled output (CLS token or mean pooling)
  - Handle different pooling strategies
  - Normalize embeddings (L2 normalization)
- [ ] Sequence length handling
  - Max sequence length enforcement (typically 512 for BERT-based)
  - Truncation strategy
  - Padding strategy

### 4. **POOLING STRATEGY**

**Current Problem:**
```csharp
// From SentenceTransformerEmbeddings.cs:75-77
var output = results.First().AsEnumerable<float>().ToArray();
// Take the first Dimension values (pooled output)
return output.Take(Dimension).ToArray();
```

**This is INCORRECT** for Sentence Transformers:
- ❌ Sentence Transformers models output token-level embeddings (shape: [batch, seq_len, hidden_dim])
- ❌ Need to apply pooling to get sentence embedding
- ❌ Different models use different pooling strategies

**What's Needed:**
- [ ] Implement pooling strategies:
  - **Mean Pooling** (most common): Average all token embeddings weighted by attention mask
  - **CLS Pooling**: Use [CLS] token embedding
  - **Max Pooling**: Max over token dimension
- [ ] Load pooling configuration from model
- [ ] Apply L2 normalization after pooling (standard for Sentence Transformers)

**Correct Implementation Example:**
```csharp
// Mean pooling with attention mask
float[] MeanPooling(float[,,] tokenEmbeddings, long[,] attentionMask)
{
    // tokenEmbeddings: [batch, seq_len, hidden_dim]
    // attentionMask: [batch, seq_len]

    var batchSize = tokenEmbeddings.GetLength(0);
    var seqLen = tokenEmbeddings.GetLength(1);
    var hiddenDim = tokenEmbeddings.GetLength(2);

    var pooled = new float[hiddenDim];
    var sumMask = 0f;

    for (int seq = 0; seq < seqLen; seq++)
    {
        var maskValue = attentionMask[0, seq];
        sumMask += maskValue;

        for (int h = 0; h < hiddenDim; h++)
        {
            pooled[h] += tokenEmbeddings[0, seq, h] * maskValue;
        }
    }

    // Average and normalize
    for (int h = 0; h < hiddenDim; h++)
    {
        pooled[h] /= Math.Max(sumMask, 1e-9f);
    }

    return L2Normalize(pooled);
}

float[] L2Normalize(float[] vector)
{
    var norm = Math.Sqrt(vector.Sum(x => x * x));
    return vector.Select(x => x / (float)norm).ToArray();
}
```

### 5. **BATCH PROCESSING OPTIMIZATION**

**Current Problem:**
```csharp
// From SentenceTransformerEmbeddings.cs:82-90
public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(...)
{
    var results = new List<float[]>();
    foreach (var text in texts)
    {
        results.Add(await EmbedAsync(text, cancellationToken));
    }
    return results;
}
```

**This is INEFFICIENT:**
- ❌ Processes one text at a time
- ❌ Doesn't leverage ONNX batching capabilities
- ❌ Makes N inference calls instead of 1

**What's Needed:**
- [ ] True batch inference
- [ ] Dynamic padding to longest sequence in batch
- [ ] Efficient tensor construction for batches
- [ ] Batch size optimization (memory vs speed)

### 6. **MODEL CONFIGURATION & METADATA**

**Missing:**
- [ ] `config.json` loading (model configuration)
- [ ] `tokenizer.json` loading (tokenizer vocabulary and config)
- [ ] `special_tokens_map.json` loading
- [ ] Model-specific parameters (max_seq_length, embedding_dim, etc.)

**Needed for:**
- Automatic dimension detection
- Proper tokenizer initialization
- Validation of inputs
- Model compatibility checks

## Development Phases

### Phase 1: Tokenizer Integration (1-2 weeks)

**Priority:** 🔴 CRITICAL

**Tasks:**
1. [ ] Add `Microsoft.ML.Tokenizers` package reference
2. [ ] Implement `ITokenizer` wrapper interface
3. [ ] Add support for loading HuggingFace tokenizer.json
4. [ ] Implement proper token ID generation
5. [ ] Add attention mask generation
6. [ ] Handle special tokens ([CLS], [SEP], [PAD])
7. [ ] Add sequence truncation/padding logic
8. [ ] Write comprehensive tokenizer tests

**Deliverables:**
- Working tokenizer compatible with Sentence Transformers
- Unit tests for tokenization
- Documentation on tokenizer usage

### Phase 2: Model Management (1 week)

**Priority:** 🟡 HIGH

**Tasks:**
1. [ ] Create model download utility
2. [ ] Implement local model caching
3. [ ] Add model validation (input/output signature checks)
4. [ ] Load model configuration files (config.json, tokenizer.json)
5. [ ] Implement model metadata API
6. [ ] Document model conversion process
7. [ ] Create list of tested models

**Deliverables:**
- Model management utilities
- Model conversion documentation
- Tested model list

### Phase 3: Correct Inference Pipeline (1-2 weeks)

**Priority:** 🔴 CRITICAL

**Tasks:**
1. [ ] Fix tensor input construction
2. [ ] Implement proper pooling strategies
3. [ ] Add L2 normalization
4. [ ] Handle model output correctly
5. [ ] Implement true batch inference
6. [ ] Add sequence length handling
7. [ ] Optimize memory usage

**Deliverables:**
- Correct embedding generation
- Performance benchmarks
- Integration tests with real models

### Phase 4: Testing & Validation (1 week)

**Priority:** 🟡 HIGH

**Tasks:**
1. [ ] End-to-end tests with real ONNX models
2. [ ] Comparison tests with Python implementation
3. [ ] Performance benchmarks
4. [ ] Memory usage profiling
5. [ ] Error handling tests
6. [ ] Documentation updates

**Deliverables:**
- Comprehensive test suite
- Performance reports
- Updated documentation

## Technical Dependencies

### Required NuGet Packages

```xml
<!-- Already added -->
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.20.1" />

<!-- NEED TO ADD -->
<PackageReference Include="Microsoft.ML.Tokenizers" Version="0.21.0" />
```

### Optional Dependencies

```xml
<!-- For GPU acceleration (optional) -->
<PackageReference Include="Microsoft.ML.OnnxRuntime.Gpu" Version="1.20.1" />

<!-- For model downloading (optional, if implementing HuggingFace Hub integration) -->
<PackageReference Include="System.Net.Http.Json" Version="9.0.0" />
```

## Reference Implementations

### Python (sentence-transformers)
```python
from sentence_transformers import SentenceTransformer

model = SentenceTransformer('all-MiniLM-L6-v2')
embeddings = model.encode(['This is a test sentence.'])
```

**What Python does:**
1. Downloads model from HuggingFace Hub
2. Loads tokenizer from `tokenizer.json`
3. Tokenizes with proper WordPiece tokenization
4. Runs ONNX/PyTorch inference
5. Applies mean pooling with attention mask
6. L2 normalizes result

### Required .NET Equivalent
```csharp
using Chonkie.Embeddings.SentenceTransformers;

// Should work like this:
var embeddings = new SentenceTransformerEmbeddings(
    modelName: "sentence-transformers/all-MiniLM-L6-v2",  // Auto-download
    cacheDir: "./models"
);

var result = await embeddings.EmbedAsync("This is a test sentence.");
// result should match Python output
```

## Testing Requirements

### Unit Tests
- [x] Constructor validation
- [x] Basic property tests
- [ ] Tokenizer integration tests
- [ ] Pooling strategy tests
- [ ] Normalization tests
- [ ] Batch processing tests

### Integration Tests
- [ ] Test with `all-MiniLM-L6-v2` (384-dim)
- [ ] Test with `all-mpnet-base-v2` (768-dim)
- [ ] Compare outputs with Python implementation
- [ ] Performance benchmarks
- [ ] Memory usage tests
- [ ] Multi-threading safety tests

### Validation Tests
- [ ] Embedding similarity matches Python
- [ ] Cosine similarity produces expected results
- [ ] Batch embeddings match individual embeddings
- [ ] Edge cases (empty string, very long text)

## Sample ONNX Models

### Recommended Starting Models

| Model | Dimension | Max Seq Length | Size | Use Case |
|-------|-----------|----------------|------|----------|
| `all-MiniLM-L6-v2` | 384 | 256 | ~90 MB | General purpose, fast |
| `all-mpnet-base-v2` | 768 | 384 | ~420 MB | Better quality |
| `paraphrase-multilingual-MiniLM-L12-v2` | 384 | 128 | ~470 MB | Multilingual |

### Model Conversion Script

Create a Python script for users:

```python
# convert_model.py
from optimum.onnxruntime import ORTModelForFeatureExtraction
from transformers import AutoTokenizer

model_name = "sentence-transformers/all-MiniLM-L6-v2"
export_path = "./models/all-MiniLM-L6-v2"

# Export model to ONNX
model = ORTModelForFeatureExtraction.from_pretrained(
    model_name,
    export=True
)
model.save_pretrained(export_path)

# Save tokenizer
tokenizer = AutoTokenizer.from_pretrained(model_name)
tokenizer.save_pretrained(export_path)

print(f"Model exported to {export_path}")
```

## Success Criteria

### Minimum Viable Product (MVP)
- [ ] Proper tokenization with `Microsoft.ML.Tokenizers`
- [ ] Correct mean pooling implementation
- [ ] L2 normalization
- [ ] Works with at least one model (`all-MiniLM-L6-v2`)
- [ ] Embeddings match Python implementation (cosine similarity > 0.99)
- [ ] Integration tests passing
- [ ] Documentation complete

### Full Production Ready
- [ ] All MVP criteria met
- [ ] Multiple models tested and documented
- [ ] Batch processing optimized
- [ ] Model auto-download (optional)
- [ ] GPU acceleration support (optional)
- [ ] Performance within 20% of Python implementation
- [ ] Memory efficient (< 2x model size)
- [ ] Thread-safe
- [ ] Comprehensive error handling
- [ ] Full API documentation

## Estimated Effort

| Phase | Priority | Effort | Timeline |
|-------|----------|--------|----------|
| Tokenizer Integration | 🔴 Critical | 2 weeks | Weeks 1-2 |
| Model Management | 🟡 High | 1 week | Week 3 |
| Correct Inference | 🔴 Critical | 2 weeks | Weeks 4-5 |
| Testing & Validation | 🟡 High | 1 week | Week 6 |
| **Total** | | **6 weeks** | **1.5 months** |

## Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Microsoft.ML.Tokenizers incompatibility | 🔴 High | Test early, have fallback to custom tokenizer |
| ONNX model incompatibility | 🟡 Medium | Stick to standard Sentence Transformers models |
| Performance issues | 🟡 Medium | Profile early, optimize hot paths |
| Memory usage | 🟡 Medium | Implement model caching, batch size limits |

## Next Steps

### Immediate Actions (This Week)
1. [ ] Add `Microsoft.ML.Tokenizers` package reference
2. [ ] Download `all-MiniLM-L6-v2` ONNX model for testing
3. [ ] Create tokenizer integration prototype
4. [ ] Write failing integration test with real model

### Short Term (Next 2 Weeks)
1. [ ] Complete tokenizer integration
2. [ ] Fix pooling and normalization
3. [ ] Validate against Python implementation
4. [ ] Update documentation

### Medium Term (Weeks 3-6)
1. [ ] Add model management utilities
2. [ ] Optimize batch processing
3. [ ] Complete test suite
4. [ ] Performance tuning

## Resources

### Documentation
- **ONNX Runtime C#:** https://onnxruntime.ai/docs/api/csharp/api/
- **Microsoft.ML.Tokenizers:** https://github.com/dotnet/machinelearning/tree/main/src/Microsoft.ML.Tokenizers
- **Sentence Transformers:** https://www.sbert.net/
- **HuggingFace Optimum:** https://huggingface.co/docs/optimum/

### Sample Code
- **ONNX Runtime Examples:** https://github.com/microsoft/onnxruntime/tree/main/csharp
- **ML.NET Tokenizers Examples:** https://github.com/dotnet/machinelearning/tree/main/docs/samples

### Model Repositories
- **HuggingFace Hub:** https://huggingface.co/sentence-transformers
- **Sentence Transformers Models:** https://www.sbert.net/docs/pretrained_models.html

---

**Last Updated:** October 24, 2025
**Next Review:** October 31, 2025
