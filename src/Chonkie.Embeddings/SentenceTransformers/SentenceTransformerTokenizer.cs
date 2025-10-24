using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.ML.Tokenizers;

namespace Chonkie.Embeddings.SentenceTransformers
{
    /// <summary>
    /// Tokenizer wrapper for Sentence Transformer models.
    /// </summary>
    public class SentenceTransformerTokenizer : IDisposable
    {
        private readonly Tokenizer? _tokenizer;
        private readonly TokenizerConfig _config;
        private readonly SpecialTokensMap _specialTokens;
        private readonly int _maxLength;
        private readonly Dictionary<string, int> _vocab;
        private readonly bool _useFallback;
        private bool _disposed;

        /// <summary>
        /// Gets the maximum sequence length.
        /// </summary>
        public int MaxLength => _maxLength;

        /// <summary>
        /// Initializes a new instance of the tokenizer.
        /// </summary>
        /// <param name="modelPath">Path to the model directory containing tokenizer files.</param>
        /// <param name="maxLength">Maximum sequence length (default: 512).</param>
        public SentenceTransformerTokenizer(string modelPath, int? maxLength = null)
        {
            if (string.IsNullOrEmpty(modelPath))
            {
                throw new ArgumentNullException(nameof(modelPath));
            }

            if (!Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"Model directory not found: {modelPath}");
            }

            // Load tokenizer configuration
            var tokenizerConfigPath = Path.Combine(modelPath, "tokenizer_config.json");
            _config = TokenizerConfig.Load(tokenizerConfigPath);

            // Load special tokens
            var specialTokensPath = Path.Combine(modelPath, "special_tokens_map.json");
            _specialTokens = SpecialTokensMap.Load(specialTokensPath);

            // Load vocabulary from vocab.txt or tokenizer.json
            _vocab = LoadVocabulary(modelPath);

            // TODO: Integration with Microsoft.ML.Tokenizers needs to be improved
            // For now, use the fallback tokenizer which works with vocab lookups
            // This is sufficient for basic BERT-style tokenization
            _tokenizer = null;
            _useFallback = true;

            // Note: Future versions should use Tokenizer.CreateBert() when available
            // in stable releases of Microsoft.ML.Tokenizers

            // Set max length
            _maxLength = maxLength ?? _config.ModelMaxLength;
            if (_maxLength <= 0)
            {
                _maxLength = 512; // Default fallback
            }
        }

        /// <summary>
        /// Loads vocabulary from vocab.txt or tokenizer.json.
        /// </summary>
        private Dictionary<string, int> LoadVocabulary(string modelPath)
        {
            var vocab = new Dictionary<string, int>();

            // Try vocab.txt first (standard for BERT models)
            var vocabTxtPath = Path.Combine(modelPath, "vocab.txt");
            if (File.Exists(vocabTxtPath))
            {
                var lines = File.ReadAllLines(vocabTxtPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    vocab[lines[i]] = i;
                }
                return vocab;
            }

            // Try tokenizer.json
            var tokenizerJsonPath = Path.Combine(modelPath, "tokenizer.json");
            if (File.Exists(tokenizerJsonPath))
            {
                try
                {
                    var json = File.ReadAllText(tokenizerJsonPath);
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("model", out var model))
                    {
                        if (model.TryGetProperty("vocab", out var vocabElement))
                        {
                            foreach (var prop in vocabElement.EnumerateObject())
                            {
                                vocab[prop.Name] = prop.Value.GetInt32();
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore errors, use default vocab
                }
            }

            // Return minimal default vocab if nothing found
            if (vocab.Count == 0)
            {
                vocab["[PAD]"] = 0;
                vocab["[UNK]"] = 100;
                vocab["[CLS]"] = 101;
                vocab["[SEP]"] = 102;
                vocab["[MASK]"] = 103;
            }

            return vocab;
        }

        /// <summary>
        /// Encodes a single text into token IDs and attention mask.
        /// </summary>
        /// <param name="text">Text to encode.</param>
        /// <param name="addSpecialTokens">Whether to add special tokens ([CLS], [SEP]).</param>
        /// <param name="maxLength">Optional override for max length.</param>
        /// <returns>Encoding result with token IDs and attention mask.</returns>
        public EncodingResult Encode(string text, bool addSpecialTokens = true, int? maxLength = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new EncodingResult
                {
                    InputIds = Array.Empty<int>(),
                    AttentionMask = Array.Empty<int>(),
                    TokenTypeIds = Array.Empty<int>()
                };
            }

            var effectiveMaxLength = maxLength ?? _maxLength;

            // Encode the text
            List<int> ids;
            if (_useFallback || _tokenizer == null)
            {
                // Use simple fallback tokenization
                ids = FallbackTokenize(text);
            }
            else
            {
                // Use Microsoft.ML.Tokenizers
                try
                {
                    ids = _tokenizer.EncodeToIds(text).ToList();
                }
                catch
                {
                    ids = FallbackTokenize(text);
                }
            }

            // Handle special tokens
            List<int> tokenIds = new();
            if (addSpecialTokens)
            {
                // Add [CLS] token at the beginning
                tokenIds.Add(GetSpecialTokenId("[CLS]"));
            }

            // Add content tokens (with truncation if needed)
            var contentLength = effectiveMaxLength - (addSpecialTokens ? 2 : 0); // Reserve space for [CLS] and [SEP]
            tokenIds.AddRange(ids.Take(contentLength));

            if (addSpecialTokens)
            {
                // Add [SEP] token at the end
                tokenIds.Add(GetSpecialTokenId("[SEP]"));
            }

            // Create attention mask (1 for real tokens)
            var attentionMask = Enumerable.Repeat(1, tokenIds.Count).ToArray();

            // Create token type IDs (0 for single sequence)
            var tokenTypeIds = Enumerable.Repeat(0, tokenIds.Count).ToArray();

            return new EncodingResult
            {
                InputIds = tokenIds.ToArray(),
                AttentionMask = attentionMask,
                TokenTypeIds = tokenTypeIds
            };
        }

        /// <summary>
        /// Fallback tokenization using simple word splitting and vocab lookup.
        /// This is used when Microsoft.ML.Tokenizers cannot load the model.
        /// </summary>
        private List<int> FallbackTokenize(string text)
        {
            var tokens = new List<int>();

            // Simple whitespace tokenization
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Try to find in vocab
                if (_vocab.TryGetValue(word, out var id))
                {
                    tokens.Add(id);
                }
                else if (_vocab.TryGetValue(word.ToLowerInvariant(), out var lowerID))
                {
                    tokens.Add(lowerID);
                }
                else
                {
                    // Use [UNK] token
                    tokens.Add(GetSpecialTokenId("[UNK]"));
                }
            }

            return tokens;
        }

        /// <summary>
        /// Encodes multiple texts into token IDs and attention masks with padding.
        /// </summary>
        /// <param name="texts">Texts to encode.</param>
        /// <param name="addSpecialTokens">Whether to add special tokens.</param>
        /// <param name="maxLength">Optional override for max length.</param>
        /// <returns>Batch encoding result with padded sequences.</returns>
        public BatchEncodingResult EncodeBatch(IEnumerable<string> texts, bool addSpecialTokens = true, int? maxLength = null)
        {
            var encodings = texts.Select(t => Encode(t, addSpecialTokens, maxLength)).ToList();

            if (encodings.Count == 0)
            {
                return new BatchEncodingResult
                {
                    InputIds = Array.Empty<int[]>(),
                    AttentionMask = Array.Empty<int[]>(),
                    TokenTypeIds = Array.Empty<int[]>()
                };
            }

            // Find the maximum length in this batch
            var batchMaxLength = encodings.Max(e => e.InputIds.Length);

            // Get padding token ID
            var padToken = _specialTokens.PadToken ?? "[PAD]";
            var padTokenId = GetSpecialTokenId(padToken);

            // Pad all sequences to the same length
            var paddedInputIds = new List<int[]>();
            var paddedAttentionMask = new List<int[]>();
            var paddedTokenTypeIds = new List<int[]>();

            foreach (var encoding in encodings)
            {
                var paddingLength = batchMaxLength - encoding.InputIds.Length;

                // Pad input IDs
                var paddedIds = encoding.InputIds.Concat(Enumerable.Repeat(padTokenId, paddingLength)).ToArray();
                paddedInputIds.Add(paddedIds);

                // Pad attention mask (0 for padding tokens)
                var paddedMask = encoding.AttentionMask.Concat(Enumerable.Repeat(0, paddingLength)).ToArray();
                paddedAttentionMask.Add(paddedMask);

                // Pad token type IDs
                var paddedTypeIds = encoding.TokenTypeIds.Concat(Enumerable.Repeat(0, paddingLength)).ToArray();
                paddedTokenTypeIds.Add(paddedTypeIds);
            }

            return new BatchEncodingResult
            {
                InputIds = paddedInputIds.ToArray(),
                AttentionMask = paddedAttentionMask.ToArray(),
                TokenTypeIds = paddedTokenTypeIds.ToArray()
            };
        }

        /// <summary>
        /// Decodes token IDs back to text.
        /// </summary>
        /// <param name="tokenIds">Token IDs to decode.</param>
        /// <param name="skipSpecialTokens">Whether to skip special tokens in output.</param>
        /// <returns>Decoded text.</returns>
        public string Decode(IEnumerable<int> tokenIds, bool skipSpecialTokens = true)
        {
            var ids = tokenIds.ToList();
            if (ids.Count == 0)
            {
                return string.Empty;
            }

            // Filter out special tokens if requested
            if (skipSpecialTokens)
            {
                var specialIds = new HashSet<int>
                {
                    GetSpecialTokenId("[PAD]"),
                    GetSpecialTokenId("[CLS]"),
                    GetSpecialTokenId("[SEP]"),
                    GetSpecialTokenId("[UNK]"),
                    GetSpecialTokenId("[MASK]")
                };
                ids = ids.Where(id => !specialIds.Contains(id)).ToList();
            }

            if (ids.Count == 0)
            {
                return string.Empty;
            }

            if (_useFallback || _tokenizer == null)
            {
                // Fallback decoding - lookup in reverse vocab
                var reverseVocab = _vocab.ToDictionary(kv => kv.Value, kv => kv.Key);
                var words = ids.Select(id => reverseVocab.TryGetValue(id, out var word) ? word : "[UNK]");
                return string.Join(" ", words);
            }

            try
            {
                return _tokenizer.Decode(ids) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Counts the number of tokens in the text (without special tokens).
        /// </summary>
        public int CountTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            if (_useFallback || _tokenizer == null)
            {
                return FallbackTokenize(text).Count;
            }

            try
            {
                var ids = _tokenizer.EncodeToIds(text);
                return ids.Count;
            }
            catch
            {
                return FallbackTokenize(text).Count;
            }
        }

        /// <summary>
        /// Gets the token ID for a special token.
        /// </summary>
        private int GetSpecialTokenId(string token)
        {
            // Try to get the token ID from the vocabulary
            if (_vocab.TryGetValue(token, out var id))
            {
                return id;
            }

            // Try encoding the token
            try
            {
                if (_tokenizer != null)
                {
                    var ids = _tokenizer.EncodeToIds(token);
                    if (ids != null && ids.Count > 0)
                    {
                        return ids[0];
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            // Fallback: use common token IDs for BERT models
            return token switch
            {
                "[PAD]" => 0,
                "[UNK]" => 100,
                "[CLS]" => 101,
                "[SEP]" => 102,
                "[MASK]" => 103,
                _ => 0
            };
        }

        /// <summary>
        /// Disposes the tokenizer resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Tokenizer in Microsoft.ML.Tokenizers might not implement IDisposable
                // So we just set disposed flag
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Result of encoding a single text.
    /// </summary>
    public class EncodingResult
    {
        /// <summary>Token IDs.</summary>
        public int[] InputIds { get; set; } = Array.Empty<int>();

        /// <summary>Attention mask (1 for real tokens, 0 for padding).</summary>
        public int[] AttentionMask { get; set; } = Array.Empty<int>();

        /// <summary>Token type IDs (segment IDs).</summary>
        public int[] TokenTypeIds { get; set; } = Array.Empty<int>();
    }

    /// <summary>
    /// Result of encoding multiple texts with padding.
    /// </summary>
    public class BatchEncodingResult
    {
        /// <summary>Batch of token IDs.</summary>
        public int[][] InputIds { get; set; } = Array.Empty<int[]>();

        /// <summary>Batch of attention masks.</summary>
        public int[][] AttentionMask { get; set; } = Array.Empty<int[]>();

        /// <summary>Batch of token type IDs.</summary>
        public int[][] TokenTypeIds { get; set; } = Array.Empty<int[]>();
    }
}
