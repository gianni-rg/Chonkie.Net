#!/usr/bin/env python3
"""
Convert Chonky token classification models to ONNX format for use with Chonkie.Net.

This script exports HuggingFace token classification models to ONNX format,
which can then be used with the C# NeuralChunker for high-performance neural chunking.

Usage:
    python convert_neural_models.py --model mirth/chonky_distilbert_base_uncased_1 --output ./models/chonky_distilbert
    
Requirements:
    pip install transformers torch optimum[onnxruntime] onnx onnxruntime
"""

import argparse
import json
import logging
import sys
from pathlib import Path
from typing import Optional

import torch
from transformers import AutoConfig, AutoModelForTokenClassification, AutoTokenizer

# Try to import optimum for ONNX export
try:
    from optimum.onnxruntime import ORTModelForTokenClassification
except ImportError:
    ORTModelForTokenClassification = None


# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


# Default Chonky models
DEFAULT_MODELS = {
    "distilbert": "mirth/chonky_distilbert_base_uncased_1",
    "modernbert-base": "mirth/chonky_modernbert_base_1",
    "modernbert-large": "mirth/chonky_modernbert_large_1",
}

# Model strides (for sliding window processing)
MODEL_STRIDES = {
    "mirth/chonky_distilbert_base_uncased_1": 256,
    "mirth/chonky_modernbert_base_1": 512,
    "mirth/chonky_modernbert_large_1": 512,
}


def convert_model_to_onnx(
    model_name: str,
    output_dir: str,
    use_optimum: bool = True,
    quantize: bool = False
) -> bool:
    """
    Convert a token classification model to ONNX format.
    
    Args:
        model_name: HuggingFace model name or path
        output_dir: Output directory for the ONNX model
        use_optimum: Use optimum library for export (recommended)
        quantize: Quantize the model for smaller size/faster inference
        
    Returns:
        True if conversion was successful, False otherwise
    """
    
    output_path = Path(output_dir)
    output_path.mkdir(parents=True, exist_ok=True)
    
    logger.info(f"Converting model: {model_name}")
    logger.info(f"Output directory: {output_path.absolute()}")
    logger.info(f"Options: optimum={use_optimum}, quantize={quantize}")
    print()
    
    # Step 1: Download and load the configuration
    logger.info("Step 1/5: Loading model configuration...")
    try:
        config = AutoConfig.from_pretrained(model_name)
        logger.info(f"✓ Configuration loaded: {config.model_type}")
        logger.info(f"  - Hidden size: {config.hidden_size}")
        logger.info(f"  - Num labels: {config.num_labels}")
        logger.info(f"  - Max position embeddings: {config.max_position_embeddings}")
    except Exception as e:
        logger.error(f"✗ Failed to load configuration: {e}")
        return False
    
    # Step 2: Load the model and tokenizer
    logger.info("\nStep 2/5: Loading model and tokenizer...")
    try:
        model = AutoModelForTokenClassification.from_pretrained(model_name)
        tokenizer = AutoTokenizer.from_pretrained(model_name)
        logger.info("✓ Model and tokenizer loaded successfully")
    except Exception as e:
        logger.error(f"✗ Failed to load model: {e}")
        return False
    
    # Step 3: Export to ONNX using optimum if available
    logger.info("\nStep 3/5: Exporting to ONNX format...")
    if use_optimum and ORTModelForTokenClassification is not None:
        try:
            ort_model = ORTModelForTokenClassification.from_pretrained(
                model_name,
                export=True
            )
            ort_model.save_pretrained(output_path)
            logger.info("✓ ONNX export successful using Optimum")
        except Exception as e:
            logger.warning(f"Optimum export failed: {e}. Trying manual export...")
            if not _export_onnx_manual(model, tokenizer, output_path):
                return False
    else:
        if not _export_onnx_manual(model, tokenizer, output_path):
            return False
    
    # Step 4: Save tokenizer
    logger.info("\nStep 4/5: Saving tokenizer...")
    try:
        tokenizer.save_pretrained(output_path)
        logger.info("✓ Tokenizer saved")
    except Exception as e:
        logger.error(f"✗ Failed to save tokenizer: {e}")
        return False
    
    # Step 5: Save configuration and metadata
    logger.info("\nStep 5/5: Saving metadata...")
    try:
        # Save config
        config_path = output_path / "config.json"
        with open(config_path, "w") as f:
            json.dump(json.loads(config.to_json_string()), f, indent=2)
        
        # Save metadata
        metadata = {
            "model_name": model_name,
            "model_type": config.model_type,
            "task": "token-classification",
            "max_position_embeddings": config.max_position_embeddings,
            "hidden_size": config.hidden_size,
            "num_labels": config.num_labels,
            "stride": MODEL_STRIDES.get(model_name, 256),
            "framework": "pt",
            "onnx_converted": True
        }
        
        metadata_path = output_path / "metadata.json"
        with open(metadata_path, "w") as f:
            json.dump(metadata, f, indent=2)
        
        logger.info("✓ Metadata saved")
    except Exception as e:
        logger.error(f"✗ Failed to save metadata: {e}")
        return False
    
    print()
    logger.info("=" * 60)
    logger.info("✓ Conversion successful!")
    logger.info(f"Model saved to: {output_path.absolute()}")
    logger.info("=" * 60)
    print()
    
    # Print usage information
    logger.info("\nUsage in Chonkie.Net:")
    logger.info("-" * 60)
    logger.info("C# example:")
    logger.info("-" * 60)
    logger.info("""
var tokenizer = new CharacterTokenizer();
var modelPath = @"./models/chonky_distilbert";

// Load with ONNX model
var chunker = new NeuralChunker(tokenizer, modelPath, chunkSize: 2048);

// Or initialize after creation
var chunker2 = new NeuralChunker(tokenizer);
chunker2.InitializeOnnxModel(modelPath);

// Chunk text
var chunks = chunker.Chunk(longText);
    """)
    
    return True


def _export_onnx_manual(model, tokenizer, output_path: Path) -> bool:
    """
    Manually export model to ONNX format using torch.onnx.export.
    """
    try:
        logger.info("Performing manual ONNX export...")
        
        # Create dummy inputs
        input_ids = torch.ones(1, 512, dtype=torch.long)
        attention_mask = torch.ones(1, 512, dtype=torch.long)
        token_type_ids = torch.zeros(1, 512, dtype=torch.long)
        
        # Export
        onnx_path = output_path / "model.onnx"
        torch.onnx.export(
            model,
            (input_ids, attention_mask, token_type_ids),
            str(onnx_path),
            input_names=["input_ids", "attention_mask", "token_type_ids"],
            output_names=["logits"],
            dynamic_axes={
                "input_ids": {0: "batch_size", 1: "sequence_length"},
                "attention_mask": {0: "batch_size", 1: "sequence_length"},
                "token_type_ids": {0: "batch_size", 1: "sequence_length"},
                "logits": {0: "batch_size", 1: "sequence_length"}
            },
            opset_version=14,
            do_constant_folding=True
        )
        
        logger.info("✓ Manual ONNX export successful")
        return True
        
    except Exception as e:
        logger.error(f"✗ Manual ONNX export failed: {e}")
        return False


def convert_all_defaults() -> None:
    """Convert all default Chonky models to ONNX."""
    logger.info("Converting all default Chonky models to ONNX format")
    logger.info("=" * 60)
    print()
    
    output_base = Path("./models")
    
    for name, model_id in DEFAULT_MODELS.items():
        output_dir = output_base / name
        logger.info(f"\n{'=' * 60}")
        logger.info(f"Converting: {model_id}")
        logger.info(f"{'=' * 60}")
        
        try:
            success = convert_model_to_onnx(
                model_id,
                str(output_dir),
                use_optimum=True,
                quantize=False
            )
            if not success:
                logger.warning(f"Failed to convert {model_id}")
        except Exception as e:
            logger.error(f"Error converting {model_id}: {e}")


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description="Convert Chonky models to ONNX format for Chonkie.Net"
    )
    
    parser.add_argument(
        "--model",
        type=str,
        help="HuggingFace model name or path to convert"
    )
    
    parser.add_argument(
        "--output",
        type=str,
        default=None,
        help="Output directory for the ONNX model"
    )
    
    parser.add_argument(
        "--all",
        action="store_true",
        help="Convert all default Chonky models"
    )
    
    parser.add_argument(
        "--no-optimum",
        action="store_true",
        help="Don't use optimum library, perform manual ONNX export"
    )
    
    parser.add_argument(
        "--quantize",
        action="store_true",
        help="Quantize the model for smaller size and faster inference"
    )
    
    parser.add_argument(
        "--list-models",
        action="store_true",
        help="List available default models"
    )
    
    args = parser.parse_args()
    
    # Handle list models
    if args.list_models:
        logger.info("Available default Chonky models:")
        for name, model_id in DEFAULT_MODELS.items():
            stride = MODEL_STRIDES.get(model_id, "unknown")
            logger.info(f"  {name:20s} -> {model_id:40s} (stride: {stride})")
        return
    
    # Handle convert all
    if args.all:
        convert_all_defaults()
        return
    
    # Handle convert single model
    if not args.model:
        parser.print_help()
        sys.exit(1)
    
    model_name = args.model
    output_dir = args.output
    
    # Auto-detect output directory if not provided
    if not output_dir:
        # Try to find matching default model
        for name, model_id in DEFAULT_MODELS.items():
            if model_id == model_name or model_name.endswith(name):
                output_dir = f"./models/{name}"
                break
        
        if not output_dir:
            # Use model name as output directory
            model_base = model_name.split("/")[-1]
            output_dir = f"./models/{model_base}"
    
    logger.info(f"Converting: {model_name}")
    logger.info(f"Output: {output_dir}")
    
    success = convert_model_to_onnx(
        model_name,
        output_dir,
        use_optimum=not args.no_optimum,
        quantize=args.quantize
    )
    
    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
