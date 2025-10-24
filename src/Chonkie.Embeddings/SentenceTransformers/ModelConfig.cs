using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chonkie.Embeddings.SentenceTransformers
{
    /// <summary>
    /// Configuration for a Sentence Transformer model loaded from config.json.
    /// </summary>
    public class ModelConfig
    {
        [JsonPropertyName("model_type")]
        public string? ModelType { get; set; }

        [JsonPropertyName("hidden_size")]
        public int HiddenSize { get; set; }

        [JsonPropertyName("max_position_embeddings")]
        public int MaxPositionEmbeddings { get; set; }

        [JsonPropertyName("vocab_size")]
        public int VocabSize { get; set; }

        [JsonPropertyName("num_hidden_layers")]
        public int NumHiddenLayers { get; set; }

        [JsonPropertyName("num_attention_heads")]
        public int NumAttentionHeads { get; set; }

        [JsonPropertyName("intermediate_size")]
        public int IntermediateSize { get; set; }

        [JsonPropertyName("hidden_act")]
        public string? HiddenAct { get; set; }

        [JsonPropertyName("hidden_dropout_prob")]
        public double HiddenDropoutProb { get; set; }

        [JsonPropertyName("attention_probs_dropout_prob")]
        public double AttentionProbsDropoutProb { get; set; }

        [JsonPropertyName("type_vocab_size")]
        public int TypeVocabSize { get; set; }

        [JsonPropertyName("initializer_range")]
        public double InitializerRange { get; set; }

        [JsonPropertyName("layer_norm_eps")]
        public double LayerNormEps { get; set; }

        [JsonPropertyName("pad_token_id")]
        public int PadTokenId { get; set; }

        [JsonPropertyName("position_embedding_type")]
        public string? PositionEmbeddingType { get; set; }

        [JsonPropertyName("use_cache")]
        public bool UseCache { get; set; }

        [JsonPropertyName("classifier_dropout")]
        public double? ClassifierDropout { get; set; }

        /// <summary>
        /// Loads model configuration from a JSON file.
        /// </summary>
        public static ModelConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Model config file not found: {configPath}");
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<ModelConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            return config ?? throw new InvalidOperationException($"Failed to parse model config from {configPath}");
        }
    }

    /// <summary>
    /// Configuration for the Sentence Transformer pooling module.
    /// </summary>
    public class PoolingConfig
    {
        [JsonPropertyName("word_embedding_dimension")]
        public int WordEmbeddingDimension { get; set; }

        [JsonPropertyName("pooling_mode_cls_token")]
        public bool PoolingModeClsToken { get; set; }

        [JsonPropertyName("pooling_mode_mean_tokens")]
        public bool PoolingModeMeanTokens { get; set; }

        [JsonPropertyName("pooling_mode_max_tokens")]
        public bool PoolingModeMaxTokens { get; set; }

        [JsonPropertyName("pooling_mode_mean_sqrt_len_tokens")]
        public bool PoolingModeMeanSqrtLenTokens { get; set; }

        [JsonPropertyName("pooling_mode_weightedmean_tokens")]
        public bool PoolingModeWeightedMeanTokens { get; set; }

        [JsonPropertyName("pooling_mode_lasttoken")]
        public bool PoolingModeLastToken { get; set; }

        /// <summary>
        /// Gets the primary pooling mode to use.
        /// </summary>
        public PoolingMode GetPrimaryPoolingMode()
        {
            if (PoolingModeMeanTokens) return PoolingMode.Mean;
            if (PoolingModeClsToken) return PoolingMode.Cls;
            if (PoolingModeMaxTokens) return PoolingMode.Max;
            if (PoolingModeLastToken) return PoolingMode.LastToken;

            // Default to mean pooling
            return PoolingMode.Mean;
        }

        /// <summary>
        /// Loads pooling configuration from a JSON file.
        /// </summary>
        public static PoolingConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                // Return default configuration if file doesn't exist
                return new PoolingConfig
                {
                    WordEmbeddingDimension = 384,
                    PoolingModeMeanTokens = true
                };
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<PoolingConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            return config ?? throw new InvalidOperationException($"Failed to parse pooling config from {configPath}");
        }
    }

    /// <summary>
    /// Special token definition that can be either a string or an object with content.
    /// </summary>
    public class SpecialToken
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// Gets the token value, handling both string and object formats.
        /// </summary>
        public string GetValue() => Content ?? string.Empty;
    }

    /// <summary>
    /// Special tokens used by the tokenizer.
    /// </summary>
    public class SpecialTokensMap
    {
        [JsonPropertyName("unk_token")]
        public JsonElement UnkTokenElement { get; set; }

        [JsonPropertyName("sep_token")]
        public JsonElement SepTokenElement { get; set; }

        [JsonPropertyName("pad_token")]
        public JsonElement PadTokenElement { get; set; }

        [JsonPropertyName("cls_token")]
        public JsonElement ClsTokenElement { get; set; }

        [JsonPropertyName("mask_token")]
        public JsonElement MaskTokenElement { get; set; }

        public string? UnkToken => ExtractToken(UnkTokenElement);
        public string? SepToken => ExtractToken(SepTokenElement);
        public string? PadToken => ExtractToken(PadTokenElement);
        public string? ClsToken => ExtractToken(ClsTokenElement);
        public string? MaskToken => ExtractToken(MaskTokenElement);

        private static string? ExtractToken(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Undefined)
                return null;

            // Handle string format: "unk_token": "[UNK]"
            if (element.ValueKind == JsonValueKind.String)
                return element.GetString();

            // Handle object format: "unk_token": { "content": "[UNK]", ... }
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("content", out var content))
                return content.GetString();

            return null;
        }

        /// <summary>
        /// Loads special tokens map from a JSON file.
        /// </summary>
        public static SpecialTokensMap Load(string tokensPath)
        {
            if (!File.Exists(tokensPath))
            {
                // Return default tokens for BERT-based models
                return new SpecialTokensMap();
            }

            var json = File.ReadAllText(tokensPath);
            var tokens = JsonSerializer.Deserialize<SpecialTokensMap>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            return tokens ?? throw new InvalidOperationException($"Failed to parse special tokens from {tokensPath}");
        }
    }

    /// <summary>
    /// Tokenizer configuration.
    /// </summary>
    public class TokenizerConfig
    {
        [JsonPropertyName("do_lower_case")]
        public bool DoLowerCase { get; set; }

        [JsonPropertyName("unk_token")]
        public string? UnkToken { get; set; }

        [JsonPropertyName("sep_token")]
        public string? SepToken { get; set; }

        [JsonPropertyName("pad_token")]
        public string? PadToken { get; set; }

        [JsonPropertyName("cls_token")]
        public string? ClsToken { get; set; }

        [JsonPropertyName("mask_token")]
        public string? MaskToken { get; set; }

        [JsonPropertyName("model_max_length")]
        public int ModelMaxLength { get; set; }

        [JsonPropertyName("tokenize_chinese_chars")]
        public bool TokenizeChineseChars { get; set; }

        [JsonPropertyName("strip_accents")]
        public bool? StripAccents { get; set; }

        /// <summary>
        /// Loads tokenizer configuration from a JSON file.
        /// </summary>
        public static TokenizerConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                // Return default configuration
                return new TokenizerConfig
                {
                    DoLowerCase = true,
                    UnkToken = "[UNK]",
                    SepToken = "[SEP]",
                    PadToken = "[PAD]",
                    ClsToken = "[CLS]",
                    MaskToken = "[MASK]",
                    ModelMaxLength = 512,
                    TokenizeChineseChars = true
                };
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<TokenizerConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            return config ?? throw new InvalidOperationException($"Failed to parse tokenizer config from {configPath}");
        }
    }

    /// <summary>
    /// Pooling modes for sentence embeddings.
    /// </summary>
    public enum PoolingMode
    {
        /// <summary>Mean pooling over all tokens with attention mask.</summary>
        Mean,

        /// <summary>Use the CLS token embedding.</summary>
        Cls,

        /// <summary>Max pooling over all tokens.</summary>
        Max,

        /// <summary>Use the last token embedding.</summary>
        LastToken
    }
}
