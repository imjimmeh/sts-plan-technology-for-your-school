﻿using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public class ComponentModelTests
    {
        private IComponentBuilder _componentBuilder;

        public ComponentModelTests()
        {
            _componentBuilder = new ComponentBuilder();
        }

        [Fact]
        public void Should_render_button_component()
        {
            var actual = _componentBuilder.BuildButtonWithLink();
            Assert.True(actual != null);
            Assert.Equal("Submit", actual.Button.Value);
            Assert.False(actual.Button.IsStartButton);
            Assert.Equal("/FakeLink", actual.Href);
        }

        [Fact]
        public void Should_render_dropdown_component()
        {
            var actual = _componentBuilder.BuildDropDownComponent();
            Assert.True(actual != null);
            Assert.Equal("Dropdown", actual.Title);
            Assert.Equal("Content", actual.Content.Value);
        }

        [Fact]
        public void Should_render_textbody_component()
        {
            var actual = _componentBuilder.BuildTextBody();
            Assert.True(actual != null);
            Assert.Equal("Content", actual.RichText.Value);
        }

        [Fact]
        public void Should_render_category_component()
        {
            var actual = _componentBuilder.BuildCategory();
            Assert.True(actual != null);
            Assert.Equal("Category", actual.Header.Text);
            Assert.True(actual.Content != null);
            Assert.Equal("Section", actual.Sections[0].Name);
            Assert.True(actual.Sections[0].Questions != null);
            Assert.Equal("Question Text", actual.Sections[0].Questions[0].Text);
            Assert.Equal("Help Text", actual.Sections[0].Questions[0].HelpText);
            Assert.True(actual.Sections[0].Questions[0].Answers[0] != null);
            Assert.Equal("Answer", actual.Sections[0].Questions[0].Answers[0].Text);
        }

    }
}