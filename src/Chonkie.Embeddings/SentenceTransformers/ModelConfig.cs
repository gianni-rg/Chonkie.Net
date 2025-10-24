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
        /// <summary>Gets or sets the model architecture type (e.g., "bert", "roberta").</summary>
        [JsonPropertyName("model_type")]
        public string? ModelType { get; set; }

        /// <summary>Gets or sets the hidden size dimension of the model.</summary>
        [JsonPropertyName("hidden_size")]
        public int HiddenSize { get; set; }

        /// <summary>Gets or sets the dimension (used by DistilBERT and some other models).</summary>
        [JsonPropertyName("dim")]
        public int Dim { get; set; }

        /// <summary>Gets the effective hidden size (prefers hidden_size, falls back to dim).</summary>
        public int EffectiveHiddenSize => HiddenSize > 0 ? HiddenSize : Dim;

        /// <summary>Gets or sets the maximum number of position embeddings.</summary>
        [JsonPropertyName("max_position_embeddings")]
        public int MaxPositionEmbeddings { get; set; }

        /// <summary>Gets or sets the vocabulary size.</summary>
        [JsonPropertyName("vocab_size")]
        public int VocabSize { get; set; }

        /// <summary>Gets or sets the number of hidden layers in the transformer.</summary>
        [JsonPropertyName("num_hidden_layers")]
        public int NumHiddenLayers { get; set; }

        /// <summary>Gets or sets the number of attention heads in each layer.</summary>
        [JsonPropertyName("num_attention_heads")]
        public int NumAttentionHeads { get; set; }

        /// <summary>Gets or sets the size of the intermediate (feed-forward) layer.</summary>
        [JsonPropertyName("intermediate_size")]
        public int IntermediateSize { get; set; }

        /// <summary>Gets or sets the activation function used in the model (e.g., "gelu").</summary>
        [JsonPropertyName("hidden_act")]
        public string? HiddenAct { get; set; }

        /// <summary>Gets or sets the dropout probability for hidden layers.</summary>
        [JsonPropertyName("hidden_dropout_prob")]
        public double HiddenDropoutProb { get; set; }

        /// <summary>Gets or sets the dropout probability for attention weights.</summary>
        [JsonPropertyName("attention_probs_dropout_prob")]
        public double AttentionProbsDropoutProb { get; set; }

        /// <summary>Gets or sets the vocabulary size for token type embeddings.</summary>
        [JsonPropertyName("type_vocab_size")]
        public int TypeVocabSize { get; set; }

        /// <summary>Gets or sets the standard deviation for weight initialization.</summary>
        [JsonPropertyName("initializer_range")]
        public double InitializerRange { get; set; }

        /// <summary>Gets or sets the epsilon value for layer normalization.</summary>
        [JsonPropertyName("layer_norm_eps")]
        public double LayerNormEps { get; set; }

        /// <summary>Gets or sets the token ID used for padding.</summary>
        [JsonPropertyName("pad_token_id")]
        public int PadTokenId { get; set; }

        /// <summary>Gets or sets the type of position embeddings (e.g., "absolute").</summary>
        [JsonPropertyName("position_embedding_type")]
        public string? PositionEmbeddingType { get; set; }

        /// <summary>Gets or sets whether to use caching for key/value states.</summary>
        [JsonPropertyName("use_cache")]
        public bool UseCache { get; set; }

        /// <summary>Gets or sets the dropout probability for the classifier head.</summary>
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
        /// <summary>Gets or sets the dimension of the word embeddings before pooling.</summary>
        [JsonPropertyName("word_embedding_dimension")]
        public int WordEmbeddingDimension { get; set; }

        /// <summary>Gets or sets whether to use CLS token pooling.</summary>
        [JsonPropertyName("pooling_mode_cls_token")]
        public bool PoolingModeClsToken { get; set; }

        /// <summary>Gets or sets whether to use mean pooling over all tokens.</summary>
        [JsonPropertyName("pooling_mode_mean_tokens")]
        public bool PoolingModeMeanTokens { get; set; }

        /// <summary>Gets or sets whether to use max pooling over all tokens.</summary>
        [JsonPropertyName("pooling_mode_max_tokens")]
        public bool PoolingModeMaxTokens { get; set; }

        /// <summary>Gets or sets whether to use mean pooling divided by square root of sequence length.</summary>
        [JsonPropertyName("pooling_mode_mean_sqrt_len_tokens")]
        public bool PoolingModeMeanSqrtLenTokens { get; set; }

        /// <summary>Gets or sets whether to use weighted mean pooling.</summary>
        [JsonPropertyName("pooling_mode_weightedmean_tokens")]
        public bool PoolingModeWeightedMeanTokens { get; set; }

        /// <summary>Gets or sets whether to use last token pooling.</summary>
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
                // Note: WordEmbeddingDimension is left at 0 so that the actual model's hidden_size will be used
                return new PoolingConfig
                {
                    WordEmbeddingDimension = 0,
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
        /// <summary>Gets or sets the content of the special token.</summary>
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
        /// <summary>Gets or sets the raw JSON element for the unknown token.</summary>
        [JsonPropertyName("unk_token")]
        public JsonElement UnkTokenElement { get; set; }

        /// <summary>Gets or sets the raw JSON element for the separator token.</summary>
        [JsonPropertyName("sep_token")]
        public JsonElement SepTokenElement { get; set; }

        /// <summary>Gets or sets the raw JSON element for the padding token.</summary>
        [JsonPropertyName("pad_token")]
        public JsonElement PadTokenElement { get; set; }

        /// <summary>Gets or sets the raw JSON element for the CLS token.</summary>
        [JsonPropertyName("cls_token")]
        public JsonElement ClsTokenElement { get; set; }

        /// <summary>Gets or sets the raw JSON element for the mask token.</summary>
        [JsonPropertyName("mask_token")]
        public JsonElement MaskTokenElement { get; set; }

        /// <summary>Gets the unknown token string value.</summary>
        public string? UnkToken => ExtractToken(UnkTokenElement);

        /// <summary>Gets the separator token string value.</summary>
        public string? SepToken => ExtractToken(SepTokenElement);

        /// <summary>Gets the padding token string value.</summary>
        public string? PadToken => ExtractToken(PadTokenElement);

        /// <summary>Gets the CLS token string value.</summary>
        public string? ClsToken => ExtractToken(ClsTokenElement);

        /// <summary>Gets the mask token string value.</summary>
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
        /// <summary>Gets or sets whether to convert tokens to lowercase.</summary>
        [JsonPropertyName("do_lower_case")]
        public bool DoLowerCase { get; set; }

        /// <summary>Gets or sets the unknown token string.</summary>
        [JsonPropertyName("unk_token")]
        public string? UnkToken { get; set; }

        /// <summary>Gets or sets the separator token string.</summary>
        [JsonPropertyName("sep_token")]
        public string? SepToken { get; set; }

        /// <summary>Gets or sets the padding token string.</summary>
        [JsonPropertyName("pad_token")]
        public string? PadToken { get; set; }

        /// <summary>Gets or sets the CLS (classification) token string.</summary>
        [JsonPropertyName("cls_token")]
        public string? ClsToken { get; set; }

        /// <summary>Gets or sets the mask token string.</summary>
        [JsonPropertyName("mask_token")]
        public string? MaskToken { get; set; }

        /// <summary>Gets or sets the maximum sequence length for the model.</summary>
        [JsonPropertyName("model_max_length")]
        public int ModelMaxLength { get; set; }

        /// <summary>Gets or sets whether to tokenize Chinese characters.</summary>
        [JsonPropertyName("tokenize_chinese_chars")]
        public bool TokenizeChineseChars { get; set; }

        /// <summary>Gets or sets whether to strip accents from characters.</summary>
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
