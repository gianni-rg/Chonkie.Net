# UV Environment Setup - Complete

## ✅ Setup Status: READY

The UV environment has been successfully configured for running the ONNX model conversion script.

## Environment Details

- **UV Version**: 0.9.5
- **Python Version**: 3.11.14
- **Virtual Environment**: `.venv` (created in `/workspace/.venv`)
- **Package Manager**: UV (fast Python package installer)

## Installed Dependencies

All required dependencies are installed and ready:

```
✓ optimum[onnxruntime] >= 1.21.0
✓ transformers >= 4.45.0
✓ sentencepiece >= 0.2.0
✓ protobuf >= 6.33.0
✓ torch 2.9.0
✓ onnxruntime 1.23.2
✓ Plus 45 other dependencies
```

## Usage

### List Available Models
```bash
uv run scripts/convert_model.py --list
```

### Convert a Model
```bash
uv run scripts/convert_model.py <model-name> [output-dir]
```

### Example: Convert MiniLM Model
```bash
# Small, fast model (384 dimensions)
uv run scripts/convert_model.py \
  sentence-transformers/all-MiniLM-L6-v2 \
  ./models/all-MiniLM-L6-v2

# High-quality model (768 dimensions)
uv run scripts/convert_model.py \
  sentence-transformers/all-mpnet-base-v2 \
  ./models/all-mpnet-base-v2
```

## Script Features

The `convert_model.py` script provides:

1. **Automatic Conversion**: Downloads and converts HuggingFace models to ONNX
2. **Configuration Extraction**: Saves model config, tokenizer config, and pooling config
3. **Validation**: Tests the converted model
4. **Usage Example**: Generates sample C# code

## Output Structure

After conversion, you'll get:

```
models/all-MiniLM-L6-v2/
├── model.onnx              # The ONNX model
├── config.json             # Model configuration
├── tokenizer.json          # Tokenizer configuration
├── tokenizer_config.json   # Tokenizer settings
├── special_tokens_map.json # Special tokens mapping
├── vocab.txt               # Vocabulary (if applicable)
└── usage_example.cs        # C# usage example
```

## Network Requirements

**Note**: The script requires internet access to download models from HuggingFace Hub. If running in a restricted network environment:

1. Pre-download models on a machine with internet access
2. Copy the model directory to your workspace
3. Use the local model path instead of the HuggingFace model name

## Verification Tests

✅ UV installation successful
✅ Virtual environment created
✅ Dependencies installed (51 packages)
✅ Script execution works (`--list` command tested)
✅ Import validation passed

## Integration with Chonkie.Net

The converted ONNX models can be used with:

```csharp
using Chonkie.Embeddings.SentenceTransformers;

var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    options: new SentenceTransformerOptions
    {
        PoolingMode = PoolingMode.Mean,
        NormalizeEmbeddings = true
    }
);

var embedding = await embeddings.EmbedAsync("Your text here");
```

## Recommended Models

### For Speed (Fast inference, lower memory)
- `sentence-transformers/all-MiniLM-L6-v2` (22MB, 384 dim)
- `sentence-transformers/all-MiniLM-L12-v2` (33MB, 384 dim)

### For Quality (Better accuracy, more memory)
- `sentence-transformers/all-mpnet-base-v2` (109MB, 768 dim)
- `sentence-transformers/all-roberta-large-v1` (355MB, 1024 dim)

### For Multilingual Support
- `sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2` (118MB, 384 dim)
- `sentence-transformers/paraphrase-multilingual-mpnet-base-v2` (278MB, 768 dim)

### For Specialized Tasks
- `sentence-transformers/msmarco-distilbert-base-v4` (Search/retrieval, 66MB, 768 dim)
- `sentence-transformers/stsb-roberta-large` (Semantic similarity, 355MB, 1024 dim)

## Troubleshooting

### If UV commands fail
```bash
# Ensure UV is on PATH
export PATH="$HOME/.local/bin:$PATH"

# Verify UV is working
uv --version
```

### If imports fail
```bash
# Reinstall dependencies
cd /workspace
uv pip install -e .
```

### If network issues occur
The devcontainer may have proxy restrictions. To work around:
1. Download models on your host machine
2. Copy to the workspace volume
3. Use local paths instead of HuggingFace model names

## Next Steps

1. ✅ **Environment Ready**: All dependencies installed
2. 🚀 **Convert Models**: Use the script to convert your chosen models
3. 🧪 **Test Integration**: Use converted models with SentenceTransformerEmbeddings
4. 📊 **Benchmark**: Compare performance with different models

## Development Workflow

The UV environment is now part of the devcontainer setup:

1. **Automatic Setup**: UV is installed via `post-create.sh`
2. **Dependency Management**: `pyproject.toml` tracks all dependencies
3. **Version Control**: Lock file ensures reproducible builds
4. **Quick Execution**: `uv run` handles activation automatically

---

**Status**: Environment is fully configured and ready for model conversion! 🎉
