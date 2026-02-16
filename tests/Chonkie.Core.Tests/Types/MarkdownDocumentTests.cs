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

namespace Chonkie.Core.Tests.Types;

using Chonkie.Core.Types;
using Xunit;

public class MarkdownDocumentTests
{
    [Fact]
    public void MarkdownDocument_CanBeCreated()
    {
        var doc = new MarkdownDocument
        {
            Content = "# Test Document"
        };

        Assert.NotNull(doc);
        Assert.Equal("# Test Document", doc.Content);
        Assert.Empty(doc.Tables);
        Assert.Empty(doc.Code);
        Assert.Empty(doc.Images);
    }

    [Fact]
    public void MarkdownDocument_SupportsTablesList()
    {
        var doc = new MarkdownDocument
        {
            Content = "Content with table",
            Tables = new List<MarkdownTable>
            {
                new MarkdownTable
                {
                    Content = "| A | B |\n|---|---|\n| 1 | 2 |",
                    StartIndex = 10,
                    EndIndex = 40
                }
            }
        };

        Assert.Single(doc.Tables);
        Assert.Equal(10, doc.Tables[0].StartIndex);
        Assert.Equal(40, doc.Tables[0].EndIndex);
    }

    [Fact]
    public void MarkdownDocument_SupportsCodeList()
    {
        var doc = new MarkdownDocument
        {
            Content = "Content with code",
            Code = new List<MarkdownCode>
            {
                new MarkdownCode
                {
                    Content = "console.log('hello');",
                    Language = "javascript",
                    StartIndex = 20,
                    EndIndex = 50
                }
            }
        };

        Assert.Single(doc.Code);
        Assert.Equal("javascript", doc.Code[0].Language);
        Assert.Equal(20, doc.Code[0].StartIndex);
    }

    [Fact]
    public void MarkdownDocument_SupportsImagesList()
    {
        var doc = new MarkdownDocument
        {
            Content = "Content with image",
            Images = new List<MarkdownImage>
            {
                new MarkdownImage
                {
                    Alias = "logo",
                    Content = "Company Logo",
                    Link = "https://example.com/logo.png",
                    StartIndex = 30,
                    EndIndex = 60
                }
            }
        };

        Assert.Single(doc.Images);
        Assert.Equal("logo", doc.Images[0].Alias);
        Assert.Equal("https://example.com/logo.png", doc.Images[0].Link);
    }

    [Fact]
    public void MarkdownDocument_InheritsFromDocument()
    {
        var doc = new MarkdownDocument
        {
            Content = "# Test",
            Source = "test.md"
        };

        Assert.IsAssignableFrom<Document>(doc);
        Assert.NotEmpty(doc.Id);
        Assert.Equal("test.md", doc.Source);
        Assert.Empty(doc.Chunks);
        Assert.Empty(doc.Metadata);
    }

    [Fact]
    public void MarkdownDocument_SupportsMultipleTables()
    {
        var doc = new MarkdownDocument
        {
            Content = "Doc with multiple tables",
            Tables = new List<MarkdownTable>
            {
                new MarkdownTable { Content = "Table 1", StartIndex = 0, EndIndex = 10 },
                new MarkdownTable { Content = "Table 2", StartIndex = 20, EndIndex = 30 }
            }
        };

        Assert.Equal(2, doc.Tables.Count);
    }

    [Fact]
    public void MarkdownTable_HasExpectedProperties()
    {
        var table = new MarkdownTable
        {
            Content = "| A | B |\n|---|---|",
            StartIndex = 100,
            EndIndex = 120
        };

        Assert.Equal("| A | B |\n|---|---|", table.Content);
        Assert.Equal(100, table.StartIndex);
        Assert.Equal(120, table.EndIndex);
    }

    [Fact]
    public void MarkdownCode_HasExpectedProperties()
    {
        var code = new MarkdownCode
        {
            Content = "def foo(): pass",
            Language = "python",
            StartIndex = 50,
            EndIndex = 70
        };

        Assert.Equal("def foo(): pass", code.Content);
        Assert.Equal("python", code.Language);
        Assert.Equal(50, code.StartIndex);
        Assert.Equal(70, code.EndIndex);
    }

    [Fact]
    public void MarkdownCode_LanguageCanBeNull()
    {
        var code = new MarkdownCode
        {
            Content = "some code",
            Language = null
        };

        Assert.Null(code.Language);
    }

    [Fact]
    public void MarkdownImage_HasExpectedProperties()
    {
        var image = new MarkdownImage
        {
            Alias = "logo",
            Content = "Logo image",
            Link = "logo.png",
            StartIndex = 10,
            EndIndex = 30
        };

        Assert.Equal("logo", image.Alias);
        Assert.Equal("Logo image", image.Content);
        Assert.Equal("logo.png", image.Link);
        Assert.Equal(10, image.StartIndex);
        Assert.Equal(30, image.EndIndex);
    }

    [Fact]
    public void MarkdownImage_LinkCanBeNull()
    {
        var image = new MarkdownImage
        {
            Alias = "img",
            Content = "Image"
        };

        Assert.Null(image.Link);
    }
}
