# Chonkie.Net Python Scripts

This directory contains Python utilities for Chonkie.Net, primarily focused on converting Sentence Transformer models to ONNX format.

## Quick Start with UV

UV is a fast Python package manager that's pre-installed in the devcontainer.

### First Time Setup

```bash
# Create a virtual environment with UV
cd /workspace
uv venv

# Activate the virtual environment
source .venv/bin/activate

# Install dependencies from pyproject.toml
uv pip install -e .
```

### Running the Model Conversion Script

```bash
# Make sure you're in the virtual environment
source /workspace/.venv/bin/activate

# Convert a model (basic usage)
python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2

# Convert to a specific directory
python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2 ./models/minilm

# List popular models
python scripts/convert_model.py --list
```

### One-Line Usage with UV (No Virtual Environment Needed)

UV can run scripts with dependencies on-the-fly:

```bash
# Run the script directly with UV
uv run scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2

# List popular models
uv run scripts/convert_model.py --list
```

## Alternative: Traditional pip

If you prefer using pip:

```bash
# Install dependencies
pip install "optimum[onnxruntime]" transformers sentencepiece protobuf

# Run the script
python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
```

## Scripts

### `convert_model.py`

Converts HuggingFace Sentence Transformer models to ONNX format for use with Chonkie.Net.

**Features:**
- Downloads models from HuggingFace Hub
- Exports to ONNX format
- Saves tokenizer and configuration files
- Validates output files

**Usage:**
```bash
python scripts/convert_model.py <model_name> [output_dir]

# Examples:
python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
python scripts/convert_model.py sentence-transformers/all-mpnet-base-v2 ./models/mpnet
python scripts/convert_model.py --list  # Show popular models
```

**Output:**
Creates a directory with:
- `model.onnx` - The ONNX model
- `config.json` - Model configuration
- `vocab.txt` - Vocabulary file
- `tokenizer_config.json` - Tokenizer configuration
- `special_tokens_map.json` - Special tokens
- `tokenizer.json` - Full tokenizer configuration

## Dependencies

All dependencies are defined in `/workspace/pyproject.toml`:

**Core Dependencies:**
- `optimum[onnxruntime]` - ONNX model conversion
- `transformers` - HuggingFace transformers
- `sentencepiece` - Tokenization
- `protobuf` - Protocol buffers

**Dev Dependencies:**
- `pytest` - Testing
- `black` - Code formatting
- `ruff` - Linting

## UV Commands Reference

```bash
# Create virtual environment
uv venv

# Install dependencies
uv pip install -e .
uv pip install -e ".[dev]"  # Include dev dependencies

# Run a script with dependencies
uv run scripts/convert_model.py <model_name>

# Update dependencies
uv pip install --upgrade optimum transformers

# List installed packages
uv pip list

# Sync environment with pyproject.toml
uv pip sync
```

## Troubleshooting

### UV Not Found

If UV is not installed:
```bash
pip install uv
```

Or install system-wide:
```bash
curl -LsSf https://astral.sh/uv/install.sh | sh
```

### Import Errors

Make sure dependencies are installed:
```bash
cd /workspace
uv pip install -e .
```

### Model Download Issues

If model download fails:
1. Check internet connectivity
2. Verify HuggingFace Hub access
3. Try a different model
4. Check proxy settings if behind a firewall

### ONNX Export Errors

If ONNX export fails:
1. Ensure all dependencies are up to date
2. Try a different model (some models may not be compatible)
3. Check available disk space

## Examples

### Convert Popular Models

```bash
# Fast, lightweight model (384 dimensions)
uv run scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2

# High quality model (768 dimensions)
uv run scripts/convert_model.py sentence-transformers/all-mpnet-base-v2

# Multilingual model (384 dimensions)
uv run scripts/convert_model.py sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2
```

### Use Converted Models in .NET

```csharp
using Chonkie.Embeddings.SentenceTransformers;

var embeddings = new SentenceTransformerEmbeddings("./models/all-MiniLM-L6-v2");
var embedding = await embeddings.EmbedAsync("Hello, world!");
```

## More Information

- [ONNX Model Conversion Guide](../docs/ONNX_MODEL_CONVERSION_GUIDE.md)
- [Chonkie.Embeddings README](../src/Chonkie.Embeddings/README.md)
- [UV Documentation](https://github.com/astral-sh/uv)
- [Sentence Transformers](https://www.sbert.net/)
