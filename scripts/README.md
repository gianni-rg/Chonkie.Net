# Chonkie.Net Python Scripts

This directory contains Python utilities for Chonkie.Net, primarily focused on converting Sentence Transformers and Neural Chunker models to ONNX format.

For ease of use, the [UV](https://github.com/astral-sh/uv) package manager is used.  
UV allows you to manage virtual environments and dependencies with simple commands.

## First Time Setup

In a Terminal, go to the project root folder and run:

```shell
uv sync
```

It will create a virtual environment, install dependencies, and set up the project for development.

## Scripts

### `convert_model.py`

Converts HuggingFace Sentence Transformers models to ONNX format for use with Chonkie.Net.

**Features:**

- Downloads models from HuggingFace Hub
- Exports to ONNX format
- Saves tokenizer and configuration files
- Validates output files

**Usage:**

```shell
uv run scripts/convert_model.py <model_name> [output_dir]

# Examples:
uv run scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
uv run scripts/convert_model.py sentence-transformers/all-mpnet-base-v2 ./models/mpnet
uv run scripts/convert_model.py --list
```

**Output:**
Creates a directory with:

- `model.onnx` - The ONNX model
- `config.json` - Model configuration
- `vocab.txt` - Vocabulary file
- `tokenizer_config.json` - Tokenizer configuration
- `special_tokens_map.json` - Special tokens
- `tokenizer.json` - Full tokenizer configuration

### Examples

#### Convert Popular Models

```shell
# Fast, lightweight model (384 dimensions)
uv run scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2

# High quality model (768 dimensions)
uv run scripts/convert_model.py sentence-transformers/all-mpnet-base-v2

# Multilingual model (384 dimensions)
uv run scripts/convert_model.py sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2
```

#### Use Converted Models in .NET

```csharp
using Chonkie.Embeddings.SentenceTransformers;

var embeddings = new SentenceTransformerEmbeddings("./models/all-MiniLM-L6-v2");
var embedding = await embeddings.EmbedAsync("Hello, world!");
```

### `convert_neural_models.py`

Converts neural chunker models (neural chunkers/sequence models used by Chonkie) to ONNX format for runtime use within Chonkie.Net.

Features:

- Downloads or accepts local neural model checkpoints (HuggingFace or local path)
- Exports the model to ONNX and saves any accompanying tokenizer/config files required by the runtime
- Optionally validates the exported ONNX model and emits simple sanity checks

Usage:

```shell
# Run the script with UV
uv run scripts/convert_neural_models.py --model <model_name_or_path> --output [output_dir]

# Show built-in or commonly used neural chunker models
uv run scripts/convert_neural_models.py --list-models
```

Examples:

```shell
# Convert a pretrained neural chunker from the hub
uv run scripts/convert_neural_models.py --model neural-chunker/some-model-name --output ./models/neural-chunker

# Convert one of the available models (distilbert, modernbert-base, or modernbert-large)
uv run scripts/convert_neural_models.py --model mirth/chonky_distilbert_base_uncased_1 --output ./models/distilbert
```

Output files:

- `model.onnx` - The exported ONNX model
- `config.json` / model metadata - configuration required by the runtime
- tokenizer files (if the model uses a tokenizer): `vocab.txt`, `tokenizer.json`, etc.
- any additional chunker-specific metadata used by Chonkie (labels, pre/post processing info)

Using converted neural chunker models in .NET

After conversion place the output folder where your application can load it (for example in `./models/neural-chunker`).

## Troubleshooting

If UV is not installed, from a Terminal, run:

```shell
# On macOS and Linux
curl -LsSf https://astral.sh/uv/install.sh | sh

# On Windows
powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
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
