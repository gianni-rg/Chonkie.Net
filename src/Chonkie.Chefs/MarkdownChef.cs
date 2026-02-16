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
using System.Threading;
using System.Threading.Tasks;
using Markdig;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Preprocesses markdown documents using Markdig.
    /// </summary>
    public class MarkdownChef : IChef
    {
        private readonly MarkdownPipeline _pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownChef"/> class.
        /// </summary>
        /// <param name="pipeline">Optional custom Markdig pipeline configuration.</param>
        public MarkdownChef(MarkdownPipeline? pipeline = null)
        {
            _pipeline = pipeline ?? new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        /// <inheritdoc />
        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(string.Empty);

            // Convert markdown to HTML (or plain text if needed)
            var html = Markdig.Markdown.ToHtml(text, _pipeline);
            return Task.FromResult(html);
        }

        /// <summary>
        /// Processes markdown text span directly.
        /// C# 14 implicit span conversion allows passing strings directly.
        /// </summary>
        /// <param name="text">The markdown text span to process.</param>
        /// <returns>Processed HTML.</returns>
        public string Process(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty || text.IsWhiteSpace())
                return string.Empty;

            // Markdig requires string, so we must allocate here
            var str = text.ToString();
            return Markdig.Markdown.ToHtml(str, _pipeline);
        }
    }
}
