using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    private readonly ICalculateMaturityCommand _calculateMaturityCommand;
    private readonly IGetResponseQuery _getResponseQuery;
    private readonly IGetQuestionQuery _getQuestionQuery;
    private readonly IGetAnswerQuery _getAnswerQuery;
    private readonly GetPageQuery _getPageQuery;

    public CheckAnswersController(ILogger<CheckAnswersController> logger, IUrlHistory history,
                                  [FromServices] ICalculateMaturityCommand calculateMaturityCommand,
                                  [FromServices] IGetResponseQuery getResponseQuery,
                                  [FromServices] IGetQuestionQuery getQuestionQuery,
                                  [FromServices] IGetAnswerQuery getAnswerQuery,
                                  [FromServices] GetPageQuery getPageQuery) : base(logger, history)
    {
        _calculateMaturityCommand = calculateMaturityCommand;
        _getResponseQuery = getResponseQuery;
        _getQuestionQuery = getQuestionQuery;
        _getAnswerQuery = getAnswerQuery;
        _getPageQuery = getPageQuery;
    }

    private async Task<Response[]?> _GetResponseList(int submissionId)
    {
        return await _getResponseQuery.GetResponseListBy(submissionId);
    }

    private async Task<Domain.Questions.Models.Question?> _GetResponseQuestion(int questionId)
    {
        return await _getQuestionQuery.GetQuestionBy(questionId);
    }

    private async Task<Domain.Answers.Models.Answer?> _GetResponseAnswer(int answerId)
    {
        return await _getAnswerQuery.GetAnswerBy(answerId);
    }

    private async Task<CheckAnswerDto> _GetCheckAnswerDto(Response[] responseList)
    {
        CheckAnswerDto checkAnswerDto = new CheckAnswerDto() { QuestionAnswerList = new QuestionWithAnswer[responseList.Length] };

        for (int i = 0; i < checkAnswerDto.QuestionAnswerList.Length; i++)
        {
            var question = await _GetResponseQuestion(responseList[i].QuestionId);
            string questionContentfulRef = question?.ContentfulRef ?? throw new NullReferenceException(nameof(question.ContentfulRef));
            string questionText = question?.QuestionText ?? throw new NullReferenceException(nameof(questionText));

            var answer = await _GetResponseAnswer(responseList[i].AnswerId);
            string answerContentfulRef = answer?.ContentfulRef ?? throw new NullReferenceException(nameof(answer.ContentfulRef));
            string answerText = answer?.AnswerText ?? throw new NullReferenceException(nameof(answerText));

            checkAnswerDto.QuestionAnswerList[i] = new QuestionWithAnswer()
            {
                QuestionRef = questionContentfulRef,
                QuestionText = questionText,
                AnswerRef = answerContentfulRef,
                AnswerText = answerText
            };
        }

        return checkAnswerDto;
    }

    private async Task<Page> _GetCheckAnswerContent()
    {
        return await _getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);
    }

    [HttpGet]
    [Route("check-answers")]
    public async Task<IActionResult> CheckAnswersPage(int submissionId, string sectionName)
    {
        Response[]? responseList = await _GetResponseList(submissionId);

        Page checkAnswerPageContent = await _GetCheckAnswerContent();

        CheckAnswersViewModel checkAnswersViewModel = new CheckAnswersViewModel()
        {
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment",
            Title = checkAnswerPageContent.Title ?? throw new NullReferenceException(nameof(checkAnswerPageContent.Title)),
            SectionName = sectionName,
            CheckAnswerDto = await _GetCheckAnswerDto(responseList ?? throw new NullReferenceException(nameof(responseList))),
            Content = checkAnswerPageContent.Content,
            SubmissionId = submissionId
        };

        return View("CheckAnswers", checkAnswersViewModel);
    }

    [HttpGet]
    [Route("change-answer")]
    public IActionResult ChangeAnswer(string questionRef, string answerRef, int submissionId)
    {
        return RedirectToAction("GetQuestionById", "Questions", new { id = questionRef, answerRef = answerRef, submissionId = submissionId });
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId)
    {
        var calculateMaturity = await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);

        if (calculateMaturity > 1)
        {
            return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });
        }

        // TODO Show error message.
        return null;
    }
}