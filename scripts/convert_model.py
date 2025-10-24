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
    required_files = ["model.onnx", "config.json", "vocab.txt", "tokenizer_config.json"]
    all_present = True

    for file in required_files:
        file_path = output_path / file
        if file_path.exists():
            size = file_path.stat().st_size / (1024 * 1024)  # Size in MB
            print(f"  ✓ {file} ({size:.2f} MB)")
        else:
            print(f"  ✗ {file} (missing)")
            all_present = False

    if all_present:
        print("\n✓ Model conversion completed successfully!")
        print(f"\nYou can now use this model in Chonkie.Net:")
        print(f'  var embeddings = new SentenceTransformerEmbeddings("{output_path.absolute()}");')
    else:
        print("\n✗ Some files are missing. The model may not work correctly.")
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
