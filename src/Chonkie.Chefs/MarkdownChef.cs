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

        public MarkdownChef(MarkdownPipeline? pipeline = null)
        {
            _pipeline = pipeline ?? new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(string.Empty);

            // Convert markdown to HTML (or plain text if needed)
            var html = Markdig.Markdown.ToHtml(text, _pipeline);
            return Task.FromResult(html);
        }
    }
}
