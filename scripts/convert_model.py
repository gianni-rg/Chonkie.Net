# Copyright 2025-2026 Gianni Rosa Gallina and Contributors
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

#!/usr/bin/env python3
"""
Script to convert Sentence Transformer models to ONNX format for use with Chonkie.Net.

Usage:
    python convert_model.py <model_name> [output_dir]

Example:
    python convert_model.py sentence-transformers/all-MiniLM-L6-v2 ./models/all-MiniLM-L6-v2

Requirements:
    pip install optimum[onnxruntime] transformers sentencepiece protobuf
"""

import sys
import os
from pathlib import Path


def convert_model(model_name: str, output_dir: str = None):
    """
    Convert a Sentence Transformer model to ONNX format.

    Args:
        model_name: HuggingFace model name (e.g., "sentence-transformers/all-MiniLM-L6-v2")
        output_dir: Output directory for the ONNX model (default: ./models/<model_name>)
    """
    try:
        from optimum.onnxruntime import ORTModelForFeatureExtraction
        from transformers import AutoTokenizer, AutoConfig
    except ImportError:
        print("ERROR: Required packages not installed.")
        print("Please run: pip install optimum[onnxruntime] transformers sentencepiece protobuf")
        sys.exit(1)

    # Set output directory
    if output_dir is None:
        model_basename = model_name.replace("/", "_")
        output_dir = f"./models/{model_basename}"

    output_path = Path(output_dir)
    output_path.mkdir(parents=True, exist_ok=True)

    print(f"Converting model: {model_name}")
    print(f"Output directory: {output_path.absolute()}")
    print()

    # Load and export the model
    print("Step 1/4: Loading model from HuggingFace Hub...")
    try:
        model = ORTModelForFeatureExtraction.from_pretrained(
            model_name,
            export=True
        )
        print("✓ Model loaded successfully")
    except Exception as e:
        print(f"✗ Failed to load model: {e}")
        sys.exit(1)

    # Save the ONNX model
    print("\nStep 2/4: Saving ONNX model...")
    try:
        model.save_pretrained(output_path)
        print("✓ ONNX model saved")
    except Exception as e:
        print(f"✗ Failed to save model: {e}")
        sys.exit(1)

    # Save the tokenizer
    print("\nStep 3/4: Saving tokenizer...")
    try:
        tokenizer = AutoTokenizer.from_pretrained(model_name)
        tokenizer.save_pretrained(output_path)
        print("✓ Tokenizer saved")
    except Exception as e:
        print(f"✗ Failed to save tokenizer: {e}")
        sys.exit(1)

    # Save configuration
    print("\nStep 4/4: Saving configuration...")
    try:
        config = AutoConfig.from_pretrained(model_name)
        config.save_pretrained(output_path)
        print("✓ Configuration saved")
    except Exception as e:
        print(f"✗ Failed to save configuration: {e}")
        sys.exit(1)

    # Verify files
    print("\nVerifying output files...")

    # Essential files that must be present
    essential_files = ["model.onnx", "config.json", "tokenizer_config.json"]

    # Tokenizer vocabulary files (different tokenizer types use different files)
    # BERT-style: vocab.txt
    # RoBERTa/GPT-2 style: vocab.json + merges.txt
    # SentencePiece: sentencepiece.bpe.model or tokenizer.model
    vocab_files = [
        "vocab.txt",           # BERT, DistilBERT
        "vocab.json",          # RoBERTa, GPT-2
        "sentencepiece.bpe.model",  # XLM-RoBERTa
        "tokenizer.model",     # T5, mT5
    ]

    all_essential_present = True
    vocab_file_present = False

    # Check essential files
    for file in essential_files:
        file_path = output_path / file
        if file_path.exists():
            size = file_path.stat().st_size / (1024 * 1024)  # Size in MB
            print(f"  ✓ {file} ({size:.2f} MB)")
        else:
            print(f"  ✗ {file} (missing)")
            all_essential_present = False

    # Check for at least one vocabulary file
    found_vocab_files = []
    for vocab_file in vocab_files:
        file_path = output_path / vocab_file
        if file_path.exists():
            size = file_path.stat().st_size / (1024 * 1024)  # Size in MB
            print(f"  ✓ {vocab_file} ({size:.2f} MB)")
            found_vocab_files.append(vocab_file)
            vocab_file_present = True

    # Check for additional tokenizer files
    additional_files = ["merges.txt", "tokenizer.json", "special_tokens_map.json", "added_tokens.json"]
    for file in additional_files:
        file_path = output_path / file
        if file_path.exists():
            size = file_path.stat().st_size / (1024 * 1024)  # Size in MB
            print(f"  ✓ {file} ({size:.2f} MB)")

    # Determine tokenizer type for user information
    if "vocab.txt" in found_vocab_files:
        tokenizer_type = "BERT-style"
    elif "vocab.json" in found_vocab_files:
        tokenizer_type = "RoBERTa/GPT-2-style"
    elif "sentencepiece.bpe.model" in found_vocab_files or "tokenizer.model" in found_vocab_files:
        tokenizer_type = "SentencePiece"
    else:
        tokenizer_type = "Unknown"

    print()
    if all_essential_present and vocab_file_present:
        print(f"✓ Model conversion completed successfully!")
        print(f"  Tokenizer type: {tokenizer_type}")
        print(f"\nYou can now use this model in Chonkie.Net:")
        print(f'  var embeddings = new SentenceTransformerEmbeddings("{output_path.absolute()}");')
    else:
        if not all_essential_present:
            print("✗ Essential files are missing. The model will not work.")
        if not vocab_file_present:
            print("✗ No vocabulary file found. The tokenizer will not work.")
        sys.exit(1)


def list_popular_models():
    """List popular Sentence Transformer models."""
    print("Popular Sentence Transformer models:")
    print()
    print("General Purpose:")
    print("  - sentence-transformers/all-MiniLM-L6-v2 (384 dim, fast)")
    print("  - sentence-transformers/all-mpnet-base-v2 (768 dim, high quality)")
    print("  - sentence-transformers/all-MiniLM-L12-v2 (384 dim, balanced)")
    print()
    print("Multilingual:")
    print("  - sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2 (384 dim)")
    print("  - sentence-transformers/paraphrase-multilingual-mpnet-base-v2 (768 dim)")
    print()
    print("Specialized:")
    print("  - sentence-transformers/msmarco-distilbert-base-v4 (768 dim, search)")
    print("  - sentence-transformers/stsb-roberta-large (1024 dim, semantic similarity)")
    print()
    print("Usage: python convert_model.py <model_name> [output_dir]")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("ERROR: Model name required.")
        print()
        list_popular_models()
        sys.exit(1)

    model_name = sys.argv[1]
    output_dir = sys.argv[2] if len(sys.argv) > 2 else None

    if model_name in ["-h", "--help", "help"]:
        list_popular_models()
        sys.exit(0)

    if model_name in ["-l", "--list", "list"]:
        list_popular_models()
        sys.exit(0)

    convert_model(model_name, output_dir)
