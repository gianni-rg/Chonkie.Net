namespace Chonkie.Chunkers.Neural;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Configuration for token classification models used by NeuralChunker.
/// Loads from the model's config.json file.
/// </summary>
public class TokenClassificationConfig
{
    /// <summary>
    /// Gets or sets the model type.
    /// </summary>
    [JsonPropertyName("model_type")]
    public string? ModelType { get; set; }

    /// <summary>
    /// Gets or sets the hidden size (embedding dimension).
    /// </summary>
    [JsonPropertyName("hidden_size")]
    public int HiddenSize { get; set; } = 768;

    /// <summary>
    /// Gets or sets the maximum position embeddings (max sequence length).
    /// </summary>
    [JsonPropertyName("max_position_embeddings")]
    public int MaxPositionEmbeddings { get; set; } = 512;

    /// <summary>
    /// Gets or sets the vocabulary size.
    /// </summary>
    [JsonPropertyName("vocab_size")]
    public int VocabSize { get; set; } = 30522;

    /// <summary>
    /// Gets or sets the number of attention heads.
    /// </summary>
    [JsonPropertyName("num_attention_heads")]
    public int NumAttentionHeads { get; set; } = 12;

    /// <summary>
    /// Gets or sets the intermediate size (feed-forward).
    /// </summary>
    [JsonPropertyName("intermediate_size")]
    public int IntermediateSize { get; set; } = 3072;

    /// <summary>
    /// Gets or sets the number of labels (classes for token classification).
    /// </summary>
    [JsonPropertyName("num_labels")]
    public int NumLabels { get; set; } = 2;

    /// <summary>
    /// Gets the label2id mapping (label name to ID).
    /// </summary>
    [JsonPropertyName("label2id")]
    public Dictionary<string, int> Label2Id { get; set; } = new();

    /// <summary>
    /// Gets the id2label mapping (label ID to name).
    /// </summary>
    [JsonPropertyName("id2label")]
    public Dictionary<string, string> Id2Label { get; set; } = new();

    /// <summary>
    /// Loads the configuration from a JSON file.
    /// </summary>
    /// <param name="configPath">Path to the config.json file.</param>
    /// <returns>The loaded configuration, or a default configuration if loading fails.</returns>
    public static TokenClassificationConfig LoadFromFile(string configPath)
    {
        try
        {
            if (!File.Exists(configPath))
            {
                return new TokenClassificationConfig();
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<TokenClassificationConfig>(json)
                ?? new TokenClassificationConfig();

            return config;
        }
        catch
        {
            // Return default configuration if loading fails
            return new TokenClassificationConfig();
        }
    }

    /// <summary>
    /// Gets the label ID for a given label name using fuzzy matching.
    /// </summary>
    /// <param name="labelName">The label name to search for.</param>
    /// <returns>The label ID, or 0 if not found.</returns>
    public int GetLabelIdByName(string labelName)
    {
        if (Label2Id.TryGetValue(labelName, out var id))
        {
            return id;
        }

        // Try lowercase matching
        var lowerName = labelName.ToLower();
        foreach (var (name, labelId) in Label2Id)
        {
            if (name.ToLower() == lowerName)
            {
                return labelId;
            }
        }

        // Try partial matching
        foreach (var (name, labelId) in Label2Id)
        {
            if (name.Contains(labelName, StringComparison.OrdinalIgnoreCase))
            {
                return labelId;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets the label name for a given label ID.
    /// </summary>
    /// <param name="labelId">The label ID.</param>
    /// <returns>The label name, or "UNKNOWN" if not found.</returns>
    public string GetLabelNameById(int labelId)
    {
        var key = labelId.ToString();
        return Id2Label.TryGetValue(key, out var name) ? name : "UNKNOWN";
    }
}
