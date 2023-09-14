using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Submission.Commands
{
    public class SubmitAnswerCommand : ISubmitAnswerCommand
    {
        private readonly GetSectionQuery _getSectionQuery;
        private readonly GetSubmitAnswerQueries _getSubmitAnswerQueries;
        private readonly RecordSubmitAnswerCommands _recordSubmitAnswerCommands;
        private readonly IGetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuery;

        public SubmitAnswerCommand(GetSectionQuery getSectionQuery, GetSubmitAnswerQueries getSubmitAnswerQueries, RecordSubmitAnswerCommands recordSubmitAnswerCommands, IGetLatestResponseListForSubmissionQuery getLatestResponseListForSubmissionQuery)
        {
            _getSectionQuery = getSectionQuery;
            _getSubmitAnswerQueries = getSubmitAnswerQueries;
            _recordSubmitAnswerCommands = recordSubmitAnswerCommands;
            _getLatestResponseListForSubmissionQuery = getLatestResponseListForSubmissionQuery;
        }

        public async Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, string sectionId, string sectionName)
        {
            int userId = await _getSubmitAnswerQueries.GetUserId();

            int submissionId = await _GetSubmissionId(submitAnswerDto.SubmissionId, sectionId, sectionName);

            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(submitAnswerDto.QuestionId, null, CancellationToken.None) ??
                            throw new KeyNotFoundException($"Could not find question for Id {submitAnswerDto.QuestionId}");

            var answer = question.Answers.FirstOrDefault(answer => answer.Sys.Id == submitAnswerDto.ChosenAnswerId) ??
                            throw new KeyNotFoundException($"Could not find answer for Id {submitAnswerDto.ChosenAnswerId} under question {submitAnswerDto.QuestionId}");

            await _recordSubmitAnswerCommands.RecordResponse(new RecordResponseDto()
            {
                UserId = userId,
                SubmissionId = submissionId,
                Question = new ContentfulReference(Id: question.Sys.Id, Text: question.Text),
                Answer = new ContentfulReference(Id: answer.Sys.Id, Text: answer.Text),
                Maturity = answer.Maturity
            });

            return submissionId;
        }

        public async Task<string?> GetNextQuestionId(string questionId, string chosenAnswerId)
        {
            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(questionId, null, CancellationToken.None);

            return question.Answers.First(answer => answer.Sys.Id.Equals(chosenAnswerId)).NextQuestion?.Sys.Id ?? null;
        }

        public async Task<bool> NextQuestionIsAnswered(int submissionId, string nextQuestionId)
        {
            var responseList = await _getSubmitAnswerQueries.GetResponseList(submissionId);

            foreach (Domain.Responses.Models.Response response in responseList ?? throw new NullReferenceException(nameof(responseList)))
            {
                Domain.Questions.Models.Question? question = await _getSubmitAnswerQueries.GetResponseQuestion(response.QuestionId) ?? throw new NullReferenceException(nameof(question));
                if (question.ContentfulRef.Equals(nextQuestionId)) return true;
            }

            return false;
        }

        public async Task<(Domain.Questionnaire.Models.Question? Question, Domain.Submissions.Models.Submission? Submission)> GetQuestionWithSubmission(
            int? submissionId,
            string? questionRef,
            string sectionId,
            string? sectionName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(questionRef)) throw new ArgumentNullException(nameof(questionRef));

            if (submissionId == null)
            {
                var submission = await _getSubmitAnswerQueries.GetSubmission(await _getSubmitAnswerQueries.GetEstablishmentId(), sectionId);

                if (submission != null && !submission.Completed)
                {
                    var nextUnansweredQuestion = await _GetNextUnansweredQuestion(submission.Id);
                    var section = await _getSectionQuery.GetSectionById(sectionId, CancellationToken.None);

                    if (nextUnansweredQuestion == null) return (null, submission);

                    if (section != null && Array.Exists(section.Questions, question => question.Sys.Id.Equals(nextUnansweredQuestion.Sys.Id))) return (nextUnansweredQuestion, submission);
                }
            }

            return (await _getSubmitAnswerQueries.GetQuestionnaireQuestion(questionRef, sectionName, cancellationToken), null);
        }

        private async Task<Domain.Questionnaire.Models.Question?> _GetNextUnansweredQuestion(int submissionId)
        {
            List<QuestionWithAnswer> questionWithAnswerList = await _getLatestResponseListForSubmissionQuery.GetResponseListByDateCreated(submissionId);

            QuestionWithAnswer latestQuestionWithAnswer = questionWithAnswerList[0];

            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(latestQuestionWithAnswer.QuestionRef, null, CancellationToken.None);

            var nextQuestion = Array.Find(question.Answers, answer => answer.Sys.Id.Equals(latestQuestionWithAnswer.AnswerRef))?.NextQuestion;

            if (nextQuestion == null) return null;

            return questionWithAnswerList.Find(questionWithAnswer => questionWithAnswer.QuestionRef.Equals(nextQuestion.Sys.Id)) == null ? nextQuestion : null;
        }

        private async Task<int> _GetSubmissionId(int? submissionId, string sectionId, string sectionName)
        {
            if (submissionId == null || submissionId == 0)
            {
                return await _recordSubmitAnswerCommands.RecordSubmission(new Domain.Submissions.Models.Submission()
                {
                    EstablishmentId = await _getSubmitAnswerQueries.GetEstablishmentId(),
                    SectionId = sectionId,
                    SectionName = sectionName
                });
            }
            else return Convert.ToUInt16(submissionId);
        }
    }
}