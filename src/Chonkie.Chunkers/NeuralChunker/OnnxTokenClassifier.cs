namespace Chonkie.Chunkers.Neural;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chonkie.Embeddings.SentenceTransformers;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

/// <summary>
/// ONNX-based token classifier for neural chunk detection.
/// Uses ONNX Runtime to run token classification models for identifying split points.
/// </summary>
public sealed class OnnxTokenClassifier : IDisposable
{
    private readonly InferenceSession _session;
    private readonly SentenceTransformerTokenizer _tokenizer;
    private readonly TokenClassificationConfig _config;
    private readonly int _maxSequenceLength;
    private readonly ILogger<OnnxTokenClassifier>? _logger;
    private bool _disposed;

    /// <summary>
    /// Gets the model configuration.
    /// </summary>
    public TokenClassificationConfig Config => _config;

    /// <summary>
    /// Gets the maximum sequence length for this model.
    /// </summary>
    public int MaxSequenceLength => _maxSequenceLength;

    /// <summary>
    /// Gets the number of labels (output classes).
    /// </summary>
    public int NumLabels => _config.NumLabels;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnnxTokenClassifier"/> class.
    /// </summary>
    /// <param name="modelPath">Path to the model directory containing the ONNX model and tokenizer files.</param>
    /// <param name="sessionOptions">Optional ONNX Runtime session options.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when model directory does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when ONNX session or tokenizer fails to initialize.</exception>
    public OnnxTokenClassifier(
        string modelPath,
        SessionOptions? sessionOptions = null,
        ILogger<OnnxTokenClassifier>? logger = null)
    {
        _logger = logger;

        if (!Directory.Exists(modelPath))
        {
            throw new DirectoryNotFoundException($"Model directory not found: {modelPath}");
        }

        var configPath = Path.Combine(modelPath, "config.json");
        _config = TokenClassificationConfig.LoadFromFile(configPath);
        _maxSequenceLength = _config.MaxPositionEmbeddings;

        _logger?.LogInformation(
            "Loaded token classification config: Hidden={Hidden}, Labels={Labels}, MaxLen={MaxLen}",
            _config.HiddenSize,
            _config.NumLabels,
            _maxSequenceLength);

        var onnxModelPath = FindOnnxModel(modelPath);
        _logger?.LogInformation("Using ONNX model: {OnnxPath}", onnxModelPath);

        sessionOptions ??= new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL
        };

        try
        {
            _session = new InferenceSession(onnxModelPath, sessionOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create ONNX inference session: {ex.Message}", ex);
        }

        try
        {
            _tokenizer = new SentenceTransformerTokenizer(modelPath, maxLength: _maxSequenceLength);
            _logger?.LogInformation("Successfully loaded tokenizer from {TokenizerPath}", modelPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load tokenizer: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Classifies tokens in the given text to identify split points.
    /// </summary>
    /// <param name="text">The text to classify.</param>
    /// <param name="stride">Optional stride for overlapping windows (for longer texts).</param>
    /// <returns>A list of token classification results.</returns>
    public List<TokenClassificationResult> Classify(string text, int stride = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<TokenClassificationResult>();
        }

        _logger?.LogDebug("Starting token classification for text of length {Length}", text.Length);

        var results = new List<TokenClassificationResult>();
        var textLength = text.Length;

        if (textLength <= _maxSequenceLength * 4)
        {
            return ClassifyChunk(text, 0);
        }

        stride = stride > 0 ? stride : _maxSequenceLength / 2;
        var charStride = stride * 4;

        var offset = 0;
        while (offset < textLength)
        {
            var chunkSize = Math.Min(charStride * 2, textLength - offset);
            var chunk = text.Substring(offset, chunkSize);
            results.AddRange(ClassifyChunk(chunk, offset));
            offset += charStride;
        }

        _logger?.LogDebug("Token classification complete: {Count} tokens classified", results.Count);
        return results;
    }

    /// <summary>
    /// Aggregates token-level classifications into span-level classifications.
    /// </summary>
    /// <param name="tokenResults">The token classification results.</param>
    /// <returns>A list of aggregated spans representing split points and non-split regions.</returns>
    public List<TokenClassificationSpan> AggregateTokens(List<TokenClassificationResult> tokenResults)
    {
        if (tokenResults.Count == 0)
        {
            return new List<TokenClassificationSpan>();
        }

        var spans = new List<TokenClassificationSpan>();
        var currentSpan = new TokenClassificationSpan
        {
            Label = ExtractEntityType(tokenResults[0].Label),
            Start = tokenResults[0].Start,
            Score = tokenResults[0].Score
        };
        currentSpan.Tokens.Add(tokenResults[0]);

        for (var i = 1; i < tokenResults.Count; i++)
        {
            var token = tokenResults[i];
            var label = token.Label;
            var entityType = ExtractEntityType(label);

            if (label.StartsWith("B-", StringComparison.OrdinalIgnoreCase) && currentSpan.Label != entityType)
            {
                currentSpan.End = tokenResults[i - 1].End;
                currentSpan.Score = currentSpan.Tokens.Average(t => t.Score);
                spans.Add(currentSpan);

                currentSpan = new TokenClassificationSpan
                {
                    Label = entityType,
                    Start = token.Start,
                    Score = token.Score
                };
                currentSpan.Tokens.Add(token);
            }
            else
            {
                currentSpan.Tokens.Add(token);
                currentSpan.Score = (currentSpan.Score + token.Score) / 2;
            }
        }

        if (currentSpan.Tokens.Count > 0)
        {
            currentSpan.End = tokenResults[^1].End;
            currentSpan.Score = currentSpan.Tokens.Average(t => t.Score);
            spans.Add(currentSpan);
        }

        return spans;
    }

    /// <summary>
    /// Disposes resources held by the ONNX session.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _session.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Classifies a single chunk of text.
    /// </summary>
    /// <param name="text">The text chunk to classify.</param>
    /// <param name="offsetInOriginal">The offset of this chunk in the original text.</param>
    /// <returns>A list of token classification results.</returns>
    private List<TokenClassificationResult> ClassifyChunk(string text, int offsetInOriginal)
    {
        var encoding = _tokenizer.Encode(text, addSpecialTokens: true);
        var inputIds = encoding.InputIds;
        var attentionMask = encoding.AttentionMask;

        var inputIdsLong = inputIds.Select(id => (long)id).ToArray();
        var attentionMaskLong = attentionMask.Select(mask => (long)mask).ToArray();

        var inputIdsTensor = CreateInputIdsTensor(new[] { inputIdsLong });
        var attentionMaskTensor = CreateAttentionMaskTensor(new[] { attentionMaskLong });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
        };

        TryAddTokenTypeIds(inputs, inputIds.Length);

        using var results = _session.Run(inputs);

        var outputName = _session.OutputMetadata.Keys.First();
        var logitsOutput = results.First(r => r.Name == outputName);
        var logitsTensor = logitsOutput.AsEnumerable<float>().ToArray();

        var sequenceLength = inputIds.Length;
        var numLabels = _config.NumLabels;

        var tokenResults = new List<TokenClassificationResult>();

        for (var i = 0; i < sequenceLength; i++)
        {
            if (i == 0 || i == sequenceLength - 1)
            {
                continue;
            }

            var tokenLogits = new float[numLabels];
            for (var j = 0; j < numLabels; j++)
            {
                tokenLogits[j] = logitsTensor[i * numLabels + j];
            }

            var labelId = 0;
            var maxLogit = tokenLogits[0];
            for (var j = 1; j < numLabels; j++)
            {
                if (tokenLogits[j] > maxLogit)
                {
                    maxLogit = tokenLogits[j];
                    labelId = j;
                }
            }

            var labelName = _config.GetLabelNameById(labelId);
            var score = GetSoftmaxScore(tokenLogits, labelId);

            var tokenText = GetTokenText(inputIds[i]);
            var approxStart = offsetInOriginal + (int)Math.Round((double)i / sequenceLength * text.Length);
            var approxEnd = offsetInOriginal + (int)Math.Round((double)(i + 1) / sequenceLength * text.Length);

            tokenResults.Add(new TokenClassificationResult
            {
                Token = tokenText,
                Score = score,
                Label = labelName,
                LabelId = labelId,
                Start = approxStart,
                End = approxEnd
            });
        }

        return tokenResults;
    }

    /// <summary>
    /// Attempts to add token_type_ids to the input list if the model expects it.
    /// </summary>
    /// <param name="inputs">The ONNX inputs collection.</param>
    /// <param name="sequenceLength">Sequence length of the current input.</param>
    private void TryAddTokenTypeIds(List<NamedOnnxValue> inputs, int sequenceLength)
    {
        try
        {
            var inputNames = _session.InputMetadata.Keys;
            if (inputNames.Contains("token_type_ids"))
            {
                var tokenTypeIdsBuffer = new long[sequenceLength];
                var tokenTypeIdsTensor = new DenseTensor<long>(tokenTypeIdsBuffer, new[] { 1, sequenceLength });
                inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor));
                _logger?.LogDebug("Added token_type_ids to inputs");
            }
        }
        catch
        {
            _logger?.LogDebug("token_type_ids not required by model");
        }
    }

    /// <summary>
    /// Creates an input_ids tensor for ONNX inference.
    /// </summary>
    private static DenseTensor<long> CreateInputIdsTensor(long[][] inputIds)
    {
        var batchSize = inputIds.Length;
        var sequenceLength = inputIds[0].Length;
        var tensor = new DenseTensor<long>(new long[batchSize * sequenceLength], new[] { batchSize, sequenceLength });

        for (var b = 0; b < batchSize; b++)
        {
            for (var s = 0; s < sequenceLength; s++)
            {
                tensor[b, s] = inputIds[b][s];
            }
        }

        return tensor;
    }

    /// <summary>
    /// Creates an attention_mask tensor for ONNX inference.
    /// </summary>
    private static DenseTensor<long> CreateAttentionMaskTensor(long[][] attentionMask)
    {
        var batchSize = attentionMask.Length;
        var sequenceLength = attentionMask[0].Length;
        var tensor = new DenseTensor<long>(new long[batchSize * sequenceLength], new[] { batchSize, sequenceLength });

        for (var b = 0; b < batchSize; b++)
        {
            for (var s = 0; s < sequenceLength; s++)
            {
                tensor[b, s] = attentionMask[b][s];
            }
        }

        return tensor;
    }

    /// <summary>
    /// Finds the ONNX model file in the model directory.
    /// </summary>
    private static string FindOnnxModel(string modelPath)
    {
        var commonNames = new[]
        {
            "model.onnx",
            "model_optimized.onnx",
            "model_quantized.onnx",
            "torch_model.onnx"
        };

        foreach (var name in commonNames)
        {
            var path = Path.Combine(modelPath, name);
            if (File.Exists(path))
            {
                return path;
            }
        }

        var onnxFiles = Directory.GetFiles(modelPath, "*.onnx");
        if (onnxFiles.Length > 0)
        {
            return onnxFiles[0];
        }

        throw new FileNotFoundException($"No ONNX model found in {modelPath}");
    }

    /// <summary>
    /// Extracts the entity type from a label (removes B- or I- prefix).
    /// </summary>
    private static string ExtractEntityType(string label)
    {
        if (label.Length > 2 && label[1] == '-')
        {
            return label.Substring(2);
        }

        return label;
    }

    /// <summary>
    /// Computes softmax score for a predicted label.
    /// </summary>
    private static float GetSoftmaxScore(float[] logits, int predictedLabelId)
    {
        var maxLogit = logits.Max();
        var expLogits = logits.Select(l => (float)Math.Exp(l - maxLogit)).ToArray();
        var sumExp = expLogits.Sum();

        return expLogits[predictedLabelId] / sumExp;
    }

    /// <summary>
    /// Gets the text representation of a token ID.
    /// </summary>
    private static string GetTokenText(int tokenId)
    {
        return $"[TOKEN_{tokenId}]";
    }
}
