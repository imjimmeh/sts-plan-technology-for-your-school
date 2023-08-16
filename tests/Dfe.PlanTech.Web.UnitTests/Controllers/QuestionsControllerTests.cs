using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
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

        private IPlanTechDbContext _databaseMock;
        private readonly QuestionsController _controller;
        private readonly SubmitAnswerCommand _submitAnswerCommand;
        private IQuestionnaireCacher _questionnaireCacherMock;
        private IGetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQueryMock;
        private readonly ICacher _cacher;

        public QuestionsControllerTests()
        {
            IContentRepository repositoryMock = MockRepository();
            _questionnaireCacherMock = MockQuestionnaireCacher();

            var mockLogger = Substitute.For<ILogger<QuestionsController>>();
            _databaseMock = Substitute.For<IPlanTechDbContext>();
            var user = Substitute.For<IUser>();

            var getQuestionnaireQuery = new Application.Questionnaire.Queries.GetQuestionQuery(_questionnaireCacherMock, repositoryMock);

            ICreateQuestionCommand createQuestionCommand = new CreateQuestionCommand(_databaseMock);
            IRecordQuestionCommand recordQuestionCommand = new RecordQuestionCommand(_databaseMock);

            IGetQuestionQuery getQuestionQuery = new Application.Submission.Queries.GetQuestionQuery(_databaseMock);
            ICreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(_databaseMock);
            IRecordAnswerCommand recordAnswerCommand = new RecordAnswerCommand(_databaseMock);
            ICreateResponseCommand createResponseCommand = new CreateResponseCommand(_databaseMock);
            IGetResponseQuery getResponseQuery = new GetResponseQuery(_databaseMock);
            IGetSubmissionQuery getSubmissionQuery = new GetSubmissionQuery(_databaseMock);
            ICreateSubmissionCommand createSubmissionCommand = new CreateSubmissionCommand(_databaseMock);
            _getLatestResponseListForSubmissionQueryMock = Substitute.For<IGetLatestResponseListForSubmissionQuery>();

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
            tempData["param"] = "admin";

            GetSubmitAnswerQueries getSubmitAnswerQueries = new GetSubmitAnswerQueries(getQuestionQuery, getResponseQuery, getSubmissionQuery, getQuestionnaireQuery, user);
            RecordSubmitAnswerCommands recordSubmitAnswerCommands = new RecordSubmitAnswerCommands(recordQuestionCommand, recordAnswerCommand, createSubmissionCommand, createResponseCommand);

            _submitAnswerCommand = new SubmitAnswerCommand(getSubmitAnswerQueries, recordSubmitAnswerCommands, _getLatestResponseListForSubmissionQueryMock);

            _controller = new QuestionsController(mockLogger) { TempData = tempData };

            _cacher = new Cacher(new CacheOptions(), new MemoryCache(new MemoryCacheOptions()));
        }

        private static IQuestionnaireCacher MockQuestionnaireCacher()
        {
            var mock = Substitute.For<IQuestionnaireCacher>();
            mock.Cached.Returns(new QuestionnaireCache());
            mock.SaveCache(Arg.Any<QuestionnaireCache>());

            return mock;
        }

        private IContentRepository MockRepository()
        {
            var repositoryMock = Substitute.For<IContentRepository>();
            repositoryMock.GetEntities<Question>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((IGetEntitiesOptions options, CancellationToken _) =>
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
            }));

            repositoryMock.GetEntityById<Question>(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                          .Returns(Task.FromResult((string id, int include, CancellationToken _) => _questions.FirstOrDefault(question => question.Sys.Id == id)));
            return repositoryMock;
        }

        [Fact]
        public async Task GetQuestionById_Should_ReturnQuestionPage_When_FetchingQuestionWithValidId()
        {
            var id = "Question1";

            var result = await _controller.GetQuestionById(id, null, 1, null, _submitAnswerCommand, CancellationToken.None);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<QuestionViewModel>(model);

            var question = model as QuestionViewModel;

            Assert.NotNull(question);
            Assert.Equal("Question One", question.Question.Text);
        }

        [Fact]
        public async Task GetQuestionById_Should_ReturnAsNormal_If_Submission_IsNull()
        {
            _controller.TempData["param"] = "SectionName+SectionId";

            var questionRef = "Question1";

            List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = -1,
                    Completed = true,
                    SectionId = "",
                    SectionName = "",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

            _databaseMock.GetSubmissions.Returns(submissionList.AsQueryable());
            _databaseMock.FirstOrDefaultAsync(submissionList.AsQueryable()).Returns(Task.FromResult(submissionList[0]));

            var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<QuestionViewModel>(model);

            var question = model as QuestionViewModel;

            Assert.NotNull(question);
            Assert.Equal("Question1", question.Question.Sys.Id);
        }

        [Fact]
        public async Task GetQuestionById_Should_ReturnAsNormal_If_PastSubmission_IsComplete()
        {
            _controller.TempData["param"] = "SectionName+SectionId";

            var questionRef = "Question1";

            List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = 0,
                    Completed = true,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

            _databaseMock.GetSubmissions.Returns(submissionList.AsQueryable());
            _databaseMock.FirstOrDefaultAsync(submissionList.AsQueryable()).Returns(Task.FromResult(submissionList[0]));

            var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<QuestionViewModel>(model);

            var question = model as QuestionViewModel;

            Assert.NotNull(question);
            Assert.Equal("Question1", question.Question.Sys.Id);
        }

        [Fact]
        public async Task GetQuestionById_Should_ReturnUnansweredQuestion_If_PastSubmission_IsNotCompleted()
        {
            _controller.TempData["param"] = "SectionName+SectionId";

            var questionRef = "Question1";

            List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = 0,
                    Completed = false,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

            List<QuestionWithAnswer> questionWithAnswerList = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = questionRef,
                    QuestionText = "Question One",
                    AnswerRef = "Answer1",
                    AnswerText = "Question 1 - Answer 1"
                }
            };

            _databaseMock.GetSubmissions.Returns(submissionList.AsQueryable());
            _databaseMock.FirstOrDefaultAsync(submissionList.AsQueryable()).Returns(Task.FromResult(submissionList[0]));

            _getLatestResponseListForSubmissionQueryMock.GetResponseListByDateCreated(1).Returns(Task.FromResult(questionWithAnswerList));

            var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<QuestionViewModel>(model);

            var question = model as QuestionViewModel;

            Assert.NotNull(question);
            Assert.Equal("Question2", question.Question.Sys.Id);
        }

        [Fact]
        public async Task GetQuestionById_Should_RedirectToCheckAnswersController_If_PastSubmission_IsNotCompleted_And_ThereIsNo_NextQuestion()
        {
            _controller.TempData["param"] = "SectionName+SectionId";

            var questionRef = "Question2";

            List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = 0,
                    Completed = false,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

            List<QuestionWithAnswer> questionWithAnswerList = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = questionRef,
                    QuestionText = "Question Two",
                    AnswerRef = "Answer4",
                    AnswerText = "Question 2 - Answer 4"
                }
            };

            _databaseMock.GetSubmissions.Returns(submissionList.AsQueryable());
            _databaseMock.FirstOrDefaultAsync(submissionList.AsQueryable()).Returns(Task.FromResult(submissionList[0]));

            _getLatestResponseListForSubmissionQueryMock.GetResponseListByDateCreated(1).Returns(Task.FromResult(questionWithAnswerList));

            var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
            Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "submissionId");
            Assert.Equal(1, id.Value);
        }

        [Fact]
        public async Task GetQuestionById_Should_ThrowException_When_IdIsNull()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionById(null!, null, null, null, _submitAnswerCommand, CancellationToken.None));
        }

        [Fact]
        public async Task GetQuestionById_Should_ThrowException_When_IdIsNotFound()
        {
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.GetQuestionById("not a real question id", null, null, null, _submitAnswerCommand, CancellationToken.None));
        }

        [Fact]
        public async Task GetQuestionById_Should_SaveSectionTitle_When_NotNull()
        {
            var id = "Question1";
            var sectionTitle = "Section title";

            await _controller.GetQuestionById(id, sectionTitle, null, null, _submitAnswerCommand, CancellationToken.None);

            _questionnaireCacherMock.Verify();
        }

        [Fact]
        public async Task GetQuestionById_Should_NotSaveSectionTitle_When_Null()
        {
            var id = "Question1";
            var result = await _controller.GetQuestionById(id, null, null, null, _submitAnswerCommand, CancellationToken.None);

            _questionnaireCacherMock.Received(0).Cached;
            _questionnaireCacherMock.Received(0).SaveCache(Arg.Any<QuestionnaireCache>());
        }

        [Fact]
        public void SubmitAnswer_Should_ThrowException_When_NullArgument()
        {
            Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.SubmitAnswer(null!, _submitAnswerCommand));
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectToNextQuestion_When_NextQuestionId_Exists()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = "Answer1",
            };

            var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

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

            var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

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
        public async void SubmitAnswer_Should_RedirectTo_CheckAnswers_When_NextQuestionIsAnswered()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = "Answer1",
                SubmissionId = 1
            };

            Domain.Questions.Models.Question question = new Domain.Questions.Models.Question()
            {
                Id = 1,
                ContentfulRef = "Question2"
            };

            _databaseMock.GetQuestion(question => question.Id == 1).Returns(Task.FromResult(question));

            _databaseMock.GetResponseList(response => response.SubmissionId == 1).Returns(Task.FromResult(
                new Domain.Responses.Models.Response[]
                {
                    new Domain.Responses.Models.Response()
                    {
                        QuestionId = 1,
                        Question = question
                    }
                }));

            var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

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

            var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

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

            var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

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

            var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

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