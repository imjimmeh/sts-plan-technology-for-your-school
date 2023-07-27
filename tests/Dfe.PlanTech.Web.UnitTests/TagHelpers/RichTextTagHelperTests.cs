using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.TagHelpers.RichText;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class RichTextTagHelperTests
{
    [Fact]
    public async Task Should_LogWarning_When_Content_Is_Null()
    {
        string content = "rich text tag";

        var loggerMock = new Mock<ILogger<RichTextTagHelper>>();
        loggerMock.Setup(LoggerMock.LogMethod<RichTextTagHelper>())
                    .Verifiable();

        var richTextRendererMock = new Mock<IRichTextRenderer>();

        var context = new TagHelperContext(tagName: "rich-text",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "richtext-test");

        var output = new TagHelperOutput("rich-text-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        var richTextTagHelper = new RichTextTagHelper(loggerMock.Object, richTextRendererMock.Object);

        await richTextTagHelper.ProcessAsync(context, output);

        loggerMock.Verify(LoggerMock.LogMethod<RichTextTagHelper>());
    }

    [Fact]
    public async Task Should_SetHtml_When_Content_Exists()
    {
        string content = "rich text tag";
        string expectedHtml = "has executed";

        var loggerMock = new Mock<ILogger<RichTextTagHelper>>();

        var richTextRendererMock = new Mock<IRichTextRenderer>();
        richTextRendererMock.Setup(renderer => renderer.ToHtml(It.IsAny<IRichTextContent>())).Returns(expectedHtml);

        var context = new TagHelperContext(tagName: "rich-text",
                                            allAttributes: new TagHelperAttributeList(),
                                            items: new Dictionary<object, object>(),
                                            uniqueId: "richtext-test");

        var output = new TagHelperOutput("rich-text-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        var richTextTagHelper = new RichTextTagHelper(loggerMock.Object, richTextRendererMock.Object)
        {
            Content = new RichTextContent()
            {
                Value = expectedHtml
            }
        };

        await richTextTagHelper.ProcessAsync(context, output);

        Assert.NotNull(richTextTagHelper.Content);
        Assert.Equal(expectedHtml, richTextTagHelper.Content.Value);
    }
}