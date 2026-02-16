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
using System.Linq;

namespace Chonkie.Embeddings.SentenceTransformers
{
    /// <summary>
    /// Pooling strategies for converting token embeddings to sentence embeddings.
    /// </summary>
    public static class PoolingUtilities
    {
        // ...existing code...
        /// <summary>
        /// Applies mean pooling over token embeddings with attention mask.
        /// </summary>
        /// <param name="tokenEmbeddings">Token embeddings [batch, seq_len, hidden_dim].</param>
        /// <param name="attentionMask">Attention mask [batch, seq_len].</param>
        /// <param name="batchSize">Batch size.</param>
        /// <param name="seqLength">Sequence length.</param>
        /// <param name="hiddenDim">Hidden dimension.</param>
        /// <returns>Pooled embeddings [batch, hidden_dim].</returns>
        public static float[][] MeanPooling(
            float[] tokenEmbeddings,
            int[] attentionMask,
            int batchSize,
            int seqLength,
            int hiddenDim)
        {
            var pooled = new float[batchSize][];

            for (int batch = 0; batch < batchSize; batch++)
            {
                pooled[batch] = new float[hiddenDim];
                var sumMask = 0f;

                // Sum embeddings weighted by attention mask
                for (int seq = 0; seq < seqLength; seq++)
                {
                    var maskIdx = batch * seqLength + seq;
                    var maskValue = attentionMask[maskIdx];
                    sumMask += maskValue;

                    for (int h = 0; h < hiddenDim; h++)
                    {
                        var embIdx = batch * seqLength * hiddenDim + seq * hiddenDim + h;
                        pooled[batch][h] += tokenEmbeddings[embIdx] * maskValue;
                    }
                }

                // Average by dividing by sum of mask
                if (sumMask > 0)
                {
                    for (int h = 0; h < hiddenDim; h++)
                    {
                        pooled[batch][h] /= sumMask;
                    }
                }
            }

            return pooled;
        }

        /// <summary>
        /// Applies CLS token pooling (uses the first token embedding).
        /// </summary>
        /// <param name="tokenEmbeddings">Token embeddings [batch, seq_len, hidden_dim].</param>
        /// <param name="batchSize">Batch size.</param>
        /// <param name="seqLength">Sequence length.</param>
        /// <param name="hiddenDim">Hidden dimension.</param>
        /// <returns>Pooled embeddings [batch, hidden_dim].</returns>
        public static float[][] ClsPooling(
            float[] tokenEmbeddings,
            int batchSize,
            int seqLength,
            int hiddenDim)
        {
            var pooled = new float[batchSize][];

            for (int batch = 0; batch < batchSize; batch++)
            {
                pooled[batch] = new float[hiddenDim];

                // Take the first token (CLS) embedding
                for (int h = 0; h < hiddenDim; h++)
                {
                    var embIdx = batch * seqLength * hiddenDim + h;
                    pooled[batch][h] = tokenEmbeddings[embIdx];
                }
            }

            return pooled;
        }

        /// <summary>
        /// Applies max pooling over token embeddings with attention mask.
        /// </summary>
        /// <param name="tokenEmbeddings">Token embeddings [batch, seq_len, hidden_dim].</param>
        /// <param name="attentionMask">Attention mask [batch, seq_len].</param>
        /// <param name="batchSize">Batch size.</param>
        /// <param name="seqLength">Sequence length.</param>
        /// <param name="hiddenDim">Hidden dimension.</param>
        /// <returns>Pooled embeddings [batch, hidden_dim].</returns>
        public static float[][] MaxPooling(
            float[] tokenEmbeddings,
            int[] attentionMask,
            int batchSize,
            int seqLength,
            int hiddenDim)
        {
            var pooled = new float[batchSize][];

            for (int batch = 0; batch < batchSize; batch++)
            {
                pooled[batch] = new float[hiddenDim];

                // Initialize with minimum float value
                for (int h = 0; h < hiddenDim; h++)
                {
                    pooled[batch][h] = float.MinValue;
                }

                // Find max values
                for (int seq = 0; seq < seqLength; seq++)
                {
                    var maskIdx = batch * seqLength + seq;
                    var maskValue = attentionMask[maskIdx];

                    // Only consider non-padding tokens
                    if (maskValue > 0)
                    {
                        for (int h = 0; h < hiddenDim; h++)
                        {
                            var embIdx = batch * seqLength * hiddenDim + seq * hiddenDim + h;
                            var value = tokenEmbeddings[embIdx];
                            if (value > pooled[batch][h])
                            {
                                pooled[batch][h] = value;
                            }
                        }
                    }
                }

                // If all were padding (shouldn't happen), set to zero
                for (int h = 0; h < hiddenDim; h++)
                {
                    if (pooled[batch][h] == float.MinValue)
                    {
                        pooled[batch][h] = 0f;
                    }
                }
            }

            return pooled;
        }

        /// <summary>
        /// Applies last token pooling (uses the last non-padding token embedding).
        /// </summary>
        /// <param name="tokenEmbeddings">Token embeddings [batch, seq_len, hidden_dim].</param>
        /// <param name="attentionMask">Attention mask [batch, seq_len].</param>
        /// <param name="batchSize">Batch size.</param>
        /// <param name="seqLength">Sequence length.</param>
        /// <param name="hiddenDim">Hidden dimension.</param>
        /// <returns>Pooled embeddings [batch, hidden_dim].</returns>
        public static float[][] LastTokenPooling(
            float[] tokenEmbeddings,
            int[] attentionMask,
            int batchSize,
            int seqLength,
            int hiddenDim)
        {
            var pooled = new float[batchSize][];

            for (int batch = 0; batch < batchSize; batch++)
            {
                pooled[batch] = new float[hiddenDim];

                // Find the last non-padding token
                var lastTokenIdx = seqLength - 1;
                for (int seq = seqLength - 1; seq >= 0; seq--)
                {
                    var maskIdx = batch * seqLength + seq;
                    if (attentionMask[maskIdx] > 0)
                    {
                        lastTokenIdx = seq;
                        break;
                    }
                }

                // Take the last token embedding
                for (int h = 0; h < hiddenDim; h++)
                {
                    var embIdx = batch * seqLength * hiddenDim + lastTokenIdx * hiddenDim + h;
                    pooled[batch][h] = tokenEmbeddings[embIdx];
                }
            }

            return pooled;
        }

        /// <summary>
        /// Applies L2 normalization to embeddings.
        /// </summary>
        /// <param name="embeddings">Embeddings to normalize [batch, hidden_dim].</param>
        /// <returns>Normalized embeddings.</returns>
        public static float[][] L2Normalize(float[][] embeddings)
        {
            var normalized = new float[embeddings.Length][];

            for (int batch = 0; batch < embeddings.Length; batch++)
            {
                var embedding = embeddings[batch];
                normalized[batch] = L2Normalize(embedding);
            }

            return normalized;
        }

        /// <summary>
        /// Applies L2 normalization to a single embedding.
        /// </summary>
        /// <param name="embedding">Embedding to normalize.</param>
        /// <returns>Normalized embedding.</returns>
        public static float[] L2Normalize(float[] embedding)
        {
            var norm = Math.Sqrt(embedding.Sum(x => x * x));

            // Avoid division by zero
            if (norm < 1e-12f)
            {
                norm = 1.0;
            }

            var normalized = new float[embedding.Length];
            for (int i = 0; i < embedding.Length; i++)
            {
                normalized[i] = embedding[i] / (float)norm;
            }

            return normalized;
        }

        /// <summary>
        /// Applies the specified pooling mode to token embeddings.
        /// </summary>
        /// <param name="tokenEmbeddings">Token embeddings [batch, seq_len, hidden_dim].</param>
        /// <param name="attentionMask">Attention mask [batch, seq_len].</param>
        /// <param name="batchSize">Batch size.</param>
        /// <param name="seqLength">Sequence length.</param>
        /// <param name="hiddenDim">Hidden dimension.</param>
        /// <param name="mode">Pooling mode to apply.</param>
        /// <param name="normalize">Whether to apply L2 normalization.</param>
        /// <returns>Pooled embeddings [batch, hidden_dim].</returns>
        public static float[][] ApplyPooling(
            float[] tokenEmbeddings,
            int[] attentionMask,
            int batchSize,
            int seqLength,
            int hiddenDim,
            PoolingMode mode,
            bool normalize = true)
        {
            float[][] pooled = mode switch
            {
                PoolingMode.Mean => MeanPooling(tokenEmbeddings, attentionMask, batchSize, seqLength, hiddenDim),
                PoolingMode.Cls => ClsPooling(tokenEmbeddings, batchSize, seqLength, hiddenDim),
                PoolingMode.Max => MaxPooling(tokenEmbeddings, attentionMask, batchSize, seqLength, hiddenDim),
                PoolingMode.LastToken => LastTokenPooling(tokenEmbeddings, attentionMask, batchSize, seqLength, hiddenDim),
                _ => throw new ArgumentException($"Unknown pooling mode: {mode}")
            };

            // Apply L2 normalization if requested
            if (normalize)
            {
                pooled = L2Normalize(pooled);
            }

            return pooled;
        }
    }
}
