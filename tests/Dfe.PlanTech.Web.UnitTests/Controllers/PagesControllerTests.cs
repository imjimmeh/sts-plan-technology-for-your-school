using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PagesControllerTests
    {
        private const string INDEX_SLUG = "/";
        private const string INDEX_TITLE = "Index";

        private readonly List<Page> _pages = new() {
            new Page()
            {
                Slug = "Landing",
                Title = new Title()
                {
                    Text = "Landing Page Title"
                },
                Content = Array.Empty<IContentComponent>()
            },
            new Page()
            {
                Slug = "Other Page",
                Title = new Title()
                {
                    Text = "Other Page Title"
                },
                Content = Array.Empty<IContentComponent>()
            },
            new Page(){
                Slug = INDEX_SLUG,
                Title = new Title(){
                    Text = INDEX_TITLE,
                },
                Content = Array.Empty<IContentComponent>()
            }
        };

        private readonly PagesController _controller;
        private readonly GetPageQuery _query;

        public PagesControllerTests()
        {
            var repositoryMock = new Mock<IContentRepository>();
            repositoryMock.Setup(repo => repo.GetEntities<Page>(It.IsAny<IEnumerable<IContentQuery>>(), It.IsAny<CancellationToken>())).ReturnsAsync((IEnumerable<IContentQuery> queries, CancellationToken cancellationToken) =>
            {
                foreach (var query in queries)
                {
                    if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                    {
                        return _pages.Where(page => page.Slug == equalsQuery.Value);
                    }
                }

                return Array.Empty<Page>();
            });

            var mockLogger = new Mock<ILogger<PagesController>>();
            _controller = new PagesController(mockLogger.Object);

            _query = new GetPageQuery(repositoryMock.Object);
        }

        [Fact]
        public async Task Should_ReturnLandingPage_When_IndexRouteLoaded()
        {
            var result = await _controller.GetByRoute(INDEX_SLUG, _query);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<Page>(model);

            var asPage = model as Page;
            Assert.Equal(INDEX_SLUG, asPage!.Slug);
            Assert.Contains(INDEX_TITLE, asPage!.Title!.Text);
        }

        [Fact]
        public async Task Should_ThrowError_When_NoRouteFound()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => _controller.GetByRoute("NOT A VALID ROUTE", _query));
        }
    }
}