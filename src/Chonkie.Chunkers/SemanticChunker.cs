// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;

namespace Chonkie.Chunkers
{
    /// <summary>
    /// SemanticChunker uses similarity-based peak detection to find split points.
    /// This chunker improves on traditional semantic chunking by using semantic similarity
    /// for boundary detection and calculating embeddings for more accurate semantic understanding.
    /// </summary>
    public class SemanticChunker : BaseChunker
    {
        private readonly IEmbeddings _embeddingModel;
        private readonly float _threshold;
        private readonly int _chunkSize;
        private readonly int _similarityWindow;
        private readonly int _minSentencesPerChunk;
        private readonly int _minCharactersPerSentence;
        private readonly string[] _delimiters;
        private readonly string _includeDelim;
        private readonly int _skipWindow;

        /// <summary>
        /// Gets the maximum size of each chunk in tokens.
        /// </summary>
        public int ChunkSize => _chunkSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticChunker"/> class.
        /// </summary>
        /// <param name="tokenizer">The tokenizer to use for token counting.</param>
        /// <param name="embeddingModel">The embedding model to use for semantic similarity.</param>
        /// <param name="logger">Logger for diagnostic output.</param>
        /// <param name="threshold">Threshold for semantic similarity (0-1). Default is 0.8.</param>
        /// <param name="chunkSize">Maximum tokens allowed per chunk. Default is 2048.</param>
        /// <param name="similarityWindow">Number of sentences to consider for similarity calculation. Default is 3.</param>
        /// <param name="minSentencesPerChunk">Minimum number of sentences per chunk. Default is 1.</param>
        /// <param name="minCharactersPerSentence">Minimum number of characters per sentence. Default is 24.</param>
        /// <param name="delimiters">Delimiters to use for sentence splitting. Default is period, exclamation, question mark, and newline.</param>
        /// <param name="includeDelim">Whether to include the delimiter in the sentence ("prev", "next", or null). Default is "prev".</param>
        /// <param name="skipWindow">Number of groups to skip when merging (0=disabled, &gt;0=enabled). Default is 0.</param>
        public SemanticChunker(
            ITokenizer tokenizer,
            IEmbeddings embeddingModel,
            ILogger<SemanticChunker>? logger = null,
            float threshold = 0.8f,
            int chunkSize = 2048,
            int similarityWindow = 3,
            int minSentencesPerChunk = 1,
            int minCharactersPerSentence = 24,
            string[]? delimiters = null,
            string includeDelim = "prev",
            int skipWindow = 0)
            : base(tokenizer, logger)
        {
            if (threshold <= 0 || threshold >= 1)
                throw new ArgumentException("Threshold must be between 0 and 1", nameof(threshold));
            if (chunkSize <= 0)
                throw new ArgumentException("ChunkSize must be positive", nameof(chunkSize));
            if (similarityWindow <= 0)
                throw new ArgumentException("SimilarityWindow must be positive", nameof(similarityWindow));
            if (minSentencesPerChunk <= 0)
                throw new ArgumentException("MinSentencesPerChunk must be positive", nameof(minSentencesPerChunk));
            if (skipWindow < 0)
                throw new ArgumentException("SkipWindow must be non-negative", nameof(skipWindow));

            _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
            _threshold = threshold;
            _chunkSize = chunkSize;
            _similarityWindow = similarityWindow;
            _minSentencesPerChunk = minSentencesPerChunk;
            _minCharactersPerSentence = minCharactersPerSentence;
            _delimiters = delimiters ?? new[] { ". ", "! ", "? ", "\n" };
            _includeDelim = includeDelim;
            _skipWindow = skipWindow;
        }

        /// <summary>
        /// Chunks the text into semantic chunks based on similarity.
        /// </summary>
        /// <param name="text">The text to chunk.</param>
        /// <returns>A list of chunks with semantic boundaries.</returns>
        public override IReadOnlyList<Chunk> Chunk(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.LogDebug("Empty or whitespace-only text provided");
                return Array.Empty<Chunk>();
            }

            Logger.LogDebug("Starting semantic chunking for text of length {Length}", text.Length);

            // Prepare sentences
            var sentences = PrepareSentences(text);
            Logger.LogDebug("Prepared {Count} sentences for semantic analysis", sentences.Count);

            // Handle edge cases - too few sentences
            if (sentences.Count <= _similarityWindow)
            {
                if (sentences.Count > 0)
                {
                    var combinedText = string.Concat(sentences.Select(s => s.Text));
                    var tokenCount = sentences.Sum(s => s.TokenCount);
                    return new[] { new Chunk
                    {
                        Text = combinedText,
                        StartIndex = 0,
                        EndIndex = combinedText.Length,
                        TokenCount = tokenCount
                    }};
                }
                return Array.Empty<Chunk>();
            }

            // Get similarity scores
            var similarities = GetSimilarityScores(sentences);

            // Get split indices based on similarity minima
            var splitIndices = GetSplitIndices(similarities);

            // Group sentences based on split points
            var sentenceGroups = GroupSentences(sentences, splitIndices);

            // Apply skip-and-merge if enabled
            if (_skipWindow > 0)
            {
                sentenceGroups = SkipAndMerge(sentenceGroups);
            }

            // Split groups that exceed chunk size
            var finalGroups = SplitOversizedGroups(sentenceGroups);

            // Create chunks from final groups
            var chunks = CreateChunks(finalGroups);

            Logger.LogInformation("Created {Count} semantic chunks from {SentenceCount} sentences",
                chunks.Count, sentences.Count);

            return chunks;
        }

        private List<Sentence> PrepareSentences(string text)
        {
            var sentenceTexts = SplitSentences(text);
            if (sentenceTexts.Count == 0)
                return new List<Sentence>();

            var tokenCounts = Tokenizer.CountTokensBatch(sentenceTexts);
            var sentences = new List<Sentence>(sentenceTexts.Count);
            int currentIndex = 0;

            for (int i = 0; i < sentenceTexts.Count; i++)
            {
                var sentenceText = sentenceTexts[i];
                sentences.Add(new Sentence
                {
                    Text = sentenceText,
                    StartIndex = currentIndex,
                    EndIndex = currentIndex + sentenceText.Length,
                    TokenCount = tokenCounts[i]
                });
                currentIndex += sentenceText.Length;
            }

            return sentences;
        }

        private List<string> SplitSentences(string text)
        {
            var sentences = new List<string>();
            var current = "";

            foreach (var delimiter in _delimiters)
            {
                var parts = text.Split(new[] { delimiter }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i < parts.Length - 1)
                    {
                        if (_includeDelim == "prev")
                            parts[i] += delimiter;
                        else if (_includeDelim == "next" && i + 1 < parts.Length)
                            parts[i + 1] = delimiter + parts[i + 1];
                    }
                }
                text = string.Join("✄", parts);
            }

            var splits = text.Split(new[] { '✄' }, StringSplitOptions.RemoveEmptyEntries);

            // Combine short splits
            foreach (var split in splits)
            {
                if (split.Length < _minCharactersPerSentence)
                {
                    current += split;
                }
                else if (!string.IsNullOrEmpty(current))
                {
                    current += split;
                    sentences.Add(current);
                    current = "";
                }
                else
                {
                    sentences.Add(split);
                }

                if (current.Length >= _minCharactersPerSentence)
                {
                    sentences.Add(current);
                    current = "";
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                sentences.Add(current);
            }

            return sentences;
        }

        private List<float> GetSimilarityScores(List<Sentence> sentences)
        {
            // Get embeddings for windows
            var windowTexts = new List<string>();
            for (int i = 0; i < sentences.Count - _similarityWindow; i++)
            {
                var windowText = string.Concat(
                    sentences.Skip(i).Take(_similarityWindow).Select(s => s.Text));
                windowTexts.Add(windowText);
            }

            // Get embeddings for sentences after the window
            var sentenceTexts = sentences.Skip(_similarityWindow).Select(s => s.Text).ToList();

            // Embed both in batch
            var windowEmbeddings = _embeddingModel.EmbedBatchAsync(windowTexts).GetAwaiter().GetResult();
            var sentenceEmbeddings = _embeddingModel.EmbedBatchAsync(sentenceTexts).GetAwaiter().GetResult();

            // Calculate similarities
            var similarities = new List<float>();
            for (int i = 0; i < windowEmbeddings.Count; i++)
            {
                var similarity = _embeddingModel.Similarity(windowEmbeddings[i], sentenceEmbeddings[i]);
                similarities.Add(similarity);
            }

            return similarities;
        }

        private List<int> GetSplitIndices(List<float> similarities)
        {
            if (similarities.Count == 0)
                return new List<int> { 0 };

            var splitIndices = new List<int> { 0 };

            // Find local minima that are below threshold
            for (int i = 1; i < similarities.Count - 1; i++)
            {
                // Check if it's a local minimum
                if (similarities[i] < similarities[i - 1] && similarities[i] < similarities[i + 1])
                {
                    // Check if it's below threshold
                    if (similarities[i] < _threshold)
                    {
                        // Check minimum distance constraint
                        if (splitIndices.Count == 0 || (i - splitIndices[^1]) >= _minSentencesPerChunk)
                        {
                            splitIndices.Add(i + _similarityWindow);
                        }
                    }
                }
            }

            // Add the end boundary
            splitIndices.Add(similarities.Count + _similarityWindow);

            return splitIndices;
        }

        private List<List<Sentence>> GroupSentences(List<Sentence> sentences, List<int> splitIndices)
        {
            var groups = new List<List<Sentence>>();

            if (splitIndices.Count == 0)
            {
                if (sentences.Count > 0)
                    groups.Add(sentences);
                return groups;
            }

            for (int i = 0; i < splitIndices.Count - 1; i++)
            {
                var group = sentences.Skip(splitIndices[i]).Take(splitIndices[i + 1] - splitIndices[i]).ToList();
                if (group.Count > 0)
                    groups.Add(group);
            }

            return groups;
        }

        private List<List<Sentence>> SkipAndMerge(List<List<Sentence>> groups)
        {
            if (groups.Count <= 1 || _skipWindow == 0)
                return groups;

            // Get embeddings for all groups
            var groupTexts = groups.Select(g => string.Concat(g.Select(s => s.Text))).ToList();
            var groupEmbeddings = _embeddingModel.EmbedBatchAsync(groupTexts).GetAwaiter().GetResult();

            var mergedGroups = new List<List<Sentence>>();
            int i = 0;

            while (i < groups.Count)
            {
                if (i == groups.Count - 1)
                {
                    mergedGroups.Add(groups[i]);
                    break;
                }

                int skipIndex = Math.Min(i + _skipWindow + 1, groups.Count - 1);
                float bestSimilarity = -1.0f;
                int bestIdx = -1;

                // Find best merge candidate within skip window
                for (int j = i + 1; j <= Math.Min(skipIndex, groups.Count - 1); j++)
                {
                    var similarity = _embeddingModel.Similarity(groupEmbeddings[i], groupEmbeddings[j]);
                    if (similarity >= _threshold && similarity > bestSimilarity)
                    {
                        bestSimilarity = similarity;
                        bestIdx = j;
                    }
                }

                if (bestIdx != -1)
                {
                    // Merge groups from i to bestIdx
                    var merged = new List<Sentence>();
                    for (int k = i; k <= bestIdx; k++)
                    {
                        merged.AddRange(groups[k]);
                    }
                    mergedGroups.Add(merged);
                    i = bestIdx + 1;
                }
                else
                {
                    mergedGroups.Add(groups[i]);
                    i++;
                }
            }

            return mergedGroups;
        }

        private List<List<Sentence>> SplitOversizedGroups(List<List<Sentence>> groups)
        {
            var finalGroups = new List<List<Sentence>>();

            foreach (var group in groups)
            {
                var tokenCount = group.Sum(s => s.TokenCount);

                if (tokenCount <= ChunkSize)
                {
                    finalGroups.Add(group);
                }
                else
                {
                    // Split into smaller chunks
                    var currentGroup = new List<Sentence>();
                    var currentTokenCount = 0;

                    foreach (var sentence in group)
                    {
                        if (currentTokenCount + sentence.TokenCount <= ChunkSize)
                        {
                            currentGroup.Add(sentence);
                            currentTokenCount += sentence.TokenCount;
                        }
                        else
                        {
                            if (currentGroup.Count > 0)
                            {
                                finalGroups.Add(currentGroup);
                            }
                            currentGroup = new List<Sentence> { sentence };
                            currentTokenCount = sentence.TokenCount;
                        }
                    }

                    if (currentGroup.Count > 0)
                    {
                        finalGroups.Add(currentGroup);
                    }
                }
            }

            return finalGroups;
        }

        private List<Chunk> CreateChunks(List<List<Sentence>> sentenceGroups)
        {
            var chunks = new List<Chunk>();
            int currentIndex = 0;

            foreach (var group in sentenceGroups)
            {
                var text = string.Concat(group.Select(s => s.Text));
                var tokenCount = group.Sum(s => s.TokenCount);
                chunks.Add(new Chunk
                {
                    Text = text,
                    StartIndex = currentIndex,
                    EndIndex = currentIndex + text.Length,
                    TokenCount = tokenCount
                });
                currentIndex += text.Length;
            }

            return chunks;
        }

        /// <summary>
        /// Gets a string representation of the SemanticChunker.
        /// </summary>
        public override string ToString()
        {
            return $"SemanticChunker(ChunkSize={ChunkSize}, Threshold={_threshold}, " +
                   $"SimilarityWindow={_similarityWindow}, MinSentencesPerChunk={_minSentencesPerChunk}, " +
                   $"SkipWindow={_skipWindow})";
        }
    }
}
