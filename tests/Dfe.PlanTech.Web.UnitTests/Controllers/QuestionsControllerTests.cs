using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class QuestionsControllerTests
    {
        private readonly List<Question> _questions = new() {
           new Question()
           {
               Sys = new SystemDetails(){
                   Id = "Question1"
               },
               Text = "Question One",
               HelpText = "Explanation",
               Answers = new[] {
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer1" },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = "Question2" } },
                       Text = "Question 1 - Answer 1"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer2" },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = "Question2" } },
                       Text = "Question 1 - Answer 2"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer3" },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = "Question2" } },
                       Text = "Question 1 - Answer 3"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer4" },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = "Question2" } },
                       Text = "Question 1 - Answer 4"
                   }
               }
           },
           new Question()
           {
               Sys = new SystemDetails(){
                   Id = "Question2"
               },
               Text = "Question Two",
               HelpText = "Explanation",
               Answers = new[] {
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer1" },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 1"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer2" },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 2"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer3" },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 3"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer4" },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 4"
                   }
               }
           }

       };

        private readonly QuestionsController _controller;
        private readonly Mock<IQuestionnaireCacher> _questionnaireCacherMock;

        private readonly ICacher _cacher;

        public QuestionsControllerTests()
        {
            Mock<IContentRepository> repositoryMock = MockRepository();
            _questionnaireCacherMock = MockQuestionnaireCacher();

            var mockLogger = new Mock<ILogger<QuestionsController>>();
            var historyMock = new Mock<IUrlHistory>();
            var databaseMock = new Mock<IPlanTechDbContext>();
            var user = new Mock<IUser>();

            GetQuestionQuery getQuestionnaireQuery = new GetQuestionQuery(_questionnaireCacherMock.Object, repositoryMock.Object);

            ICreateQuestionCommand createQuestionCommand = new CreateQuestionCommand(databaseMock.Object);
            IRecordQuestionCommand recordQuestionCommand = new RecordQuestionCommand(databaseMock.Object);

            IGetQuestionQuery getQuestionQuery = new Application.Submission.Queries.GetQuestionQuery(databaseMock.Object);
            ICreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(databaseMock.Object);
            IRecordAnswerCommand recordAnswerCommand = new RecordAnswerCommand(databaseMock.Object);
            ICreateResponseCommand createResponseCommand = new CreateResponseCommand(databaseMock.Object);
            IGetResponseQuery getResponseQuery = new GetResponseQuery(databaseMock.Object);
            ICreateSubmissionCommand createSubmissionCommand = new CreateSubmissionCommand(databaseMock.Object);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["param"] = "admin";

            _controller = new QuestionsController(mockLogger.Object, historyMock.Object, getQuestionnaireQuery, getQuestionQuery, recordQuestionCommand, recordAnswerCommand, createSubmissionCommand, createResponseCommand, getResponseQuery, user.Object)
            {
                TempData = tempData
            };

            _cacher = new Cacher(new CacheOptions(), new MemoryCache(new MemoryCacheOptions()));
        }

        private static Mock<IQuestionnaireCacher> MockQuestionnaireCacher()
        {
            var mock = new Mock<IQuestionnaireCacher>();
            mock.Setup(questionnaireCache => questionnaireCache.Cached).Returns(new QuestionnaireCache()).Verifiable();
            mock.Setup(questionnaireCache => questionnaireCache.SaveCache(It.IsAny<QuestionnaireCache>())).Verifiable();

            return mock;
        }

        private Mock<IContentRepository> MockRepository()
        {
            var repositoryMock = new Mock<IContentRepository>();
            repositoryMock.Setup(repo => repo.GetEntities<Question>(It.IsAny<IGetEntitiesOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync((IGetEntitiesOptions options, CancellationToken _) =>
            {
                if (options?.Queries != null)
                {
                    foreach (var query in options.Queries)
                    {
                        if (query is ContentQueryEquals equalsQuery && query.Field == "sys.id")
                        {
                            return _questions.Where(question => question.Sys.Id == equalsQuery.Value);
                        }
                    }
                }

                return Array.Empty<Question>();
            });

            repositoryMock.Setup(repo => repo.GetEntityById<Question>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((string id, int include, CancellationToken _) => _questions.FirstOrDefault(question => question.Sys.Id == id));
            return repositoryMock;
        }

        [Fact]
        public async Task GetQuestionById_Should_ReturnQuestionPage_When_FetchingQuestionWithValidId()
        {
            var id = "Question1";

            var result = await _controller.GetQuestionById(id, null, null, null, CancellationToken.None);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<QuestionViewModel>(model);

            var question = model as QuestionViewModel;

            Assert.NotNull(question);
            Assert.Equal("Question One", question.Question.Text);
        }

        [Fact]
        public async Task GetQuestionById_Should_ThrowException_When_IdIsNull()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionById(null!, null, null, null, CancellationToken.None));
        }

        [Fact]
        public async Task GetQuestionById_Should_ThrowException_When_IdIsNotFound()
        {
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.GetQuestionById("not a real question id", null, null, null, CancellationToken.None));
        }

        [Fact]
        public async Task GetQuestionById_Should_SaveSectionTitle_When_NotNull()
        {
            var id = "Question1";
            var sectionTitle = "Section title";

            await _controller.GetQuestionById(id, sectionTitle, null, null, CancellationToken.None);

            _questionnaireCacherMock.Verify();
        }

        [Fact]
        public async Task GetQuestionById_Should_NotSaveSectionTitle_When_Null()
        {
            var id = "Question1";
            var result = await _controller.GetQuestionById(id, null, null, null, CancellationToken.None);

            _questionnaireCacherMock.Verify(cache => cache.Cached, Times.Never);
            _questionnaireCacherMock.Verify(cache => cache.SaveCache(It.IsAny<QuestionnaireCache>()), Times.Never);
        }

        [Fact]
        public void SubmitAnswer_Should_ThrowException_When_NullArgument()
        {
            Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.SubmitAnswer(null!));
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectToNextQuestion_When_NextQuestionId_Exists()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = "Answer1",
            };

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
            Assert.Equal("Question2", id.Value);
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectTo_CheckAnswers_When_NextQuestionId_IsNull()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question2",
                ChosenAnswerId = "Answer1",
                SubmissionId = 1
            };

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
            Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "submissionId");
            Assert.Equal(submitAnswerDto.SubmissionId, id.Value);
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectTo_SameQuestion_When_ChosenAnswerId_IsNull()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = null!,
            };

            _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
            Assert.Equal(submitAnswerDto.QuestionId, id.Value);
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectTo_SameQuestion_When_NextQuestionId_And_ChosenAnswerId_IsNull()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = null!
            };

            _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
            Assert.Equal(submitAnswerDto.QuestionId, id.Value);
        }

        [Fact]
        public async void SubmitAnswer_Params_Should_Parse_When_Params_IsNotNull()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = "Answer1",
                Params = "SectionName+SectionId"
            };

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
            Assert.Equal("Question2", id.Value);
        }
    }
}