using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers
{
    public class TaskListTagTagHelperTests
    {
        [Fact]
        public void Should_Create_CorrectColour_When_ValidColour()
        {
            var colour = "Grey";

            var tagHelper = new TaskListTagTagHelper
            {
                Colour = colour
            };

            var context = new TagHelperContext(tagName: "task-list-tag",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "tasklisttag-test");

            var output = new TagHelperOutput("task-list-tag",
                                            attributes: new TagHelperAttributeList(),
                                            getChildContentAsync: (useCachedResult, encoder) =>
                                            {
                                                var tagHelperContent = new DefaultTagHelperContent();
                                                tagHelperContent.SetContent("task list tag");
                                                return Task.FromResult<TagHelperContent>(tagHelperContent);
                                            });

            tagHelper.Process(context, output);

            var htmlString = output.ToHtmlString();

            Assert.Contains($"class=\"govuk-tag govuk-tag--{colour.ToLower()}\"", htmlString);
        }

        [Fact]
        public void Should_Float_To_Right()
        {
            var colour = "Grey";

            var tagHelper = new TaskListTagTagHelper
            {
                Colour = colour
            };

            var context = new TagHelperContext(tagName: "task-list-tag",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "tasklisttag-test");

            var output = new TagHelperOutput("task-list-tag",
                                            attributes: new TagHelperAttributeList(),
                                            getChildContentAsync: (useCachedResult, encoder) =>
                                            {
                                                var tagHelperContent = new DefaultTagHelperContent();
                                                tagHelperContent.SetContent("task list tag");
                                                return Task.FromResult<TagHelperContent>(tagHelperContent);
                                            });

            tagHelper.Process(context, output);

            var htmlString = output.ToHtmlString();

            Assert.Contains($"style=\"float: right;\"", htmlString);
        }

        [Fact]
        public void Should_Create_DefaultColour_When_InvalidColour()
        {
            var colour = "Clementine";

            var tagHelper = new TaskListTagTagHelper
            {
                Colour = colour
            };

            var context = new TagHelperContext(tagName: "task-list-tag",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "tasklisttag-test");

            var output = new TagHelperOutput("task-list-tag",
                                            attributes: new TagHelperAttributeList(),
                                            getChildContentAsync: (useCachedResult, encoder) =>
                                            {
                                                var tagHelperContent = new DefaultTagHelperContent();
                                                tagHelperContent.SetContent("task list tag");
                                                return Task.FromResult<TagHelperContent>(tagHelperContent);
                                            });

            tagHelper.Process(context, output);

            var htmlString = output.ToHtmlString();

            Assert.Contains("class=\"govuk-tag\"", htmlString);
        }

        [Fact]
        public void Should_Be_Strong_Tag()
        {
            var tagHelper = new TaskListTagTagHelper
            {
                Colour = "Grey"
            };

            var context = new TagHelperContext(tagName: "task-list-tag",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "tasklisttag-test");

            var output = new TagHelperOutput("task-list-tag",
                                            attributes: new TagHelperAttributeList(),
                                            getChildContentAsync: (useCachedResult, encoder) =>
                                            {
                                                var tagHelperContent = new DefaultTagHelperContent();
                                                tagHelperContent.SetContent("task list tag");
                                                return Task.FromResult<TagHelperContent>(tagHelperContent);
                                            });

            tagHelper.Process(context, output);

            var htmlString = output.ToHtmlString();

            Assert.StartsWith("<strong", htmlString);
            Assert.EndsWith("</strong>", htmlString);
        }
    }
}