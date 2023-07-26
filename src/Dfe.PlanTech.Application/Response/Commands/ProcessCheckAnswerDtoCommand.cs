using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Response.Commands
{
    public class ProcessCheckAnswerDtoCommand
    {
        private readonly IGetQuestionQuery _getQuestionQuery;
        private readonly IGetAnswerQuery _getAnswerQuery;
        private readonly GetQuestionQuery _getQuestionnaireQuery;
        private readonly IGetResponseQuery _getResponseQuery;
        private readonly ICalculateMaturityCommand _calculateMaturityCommand;

        public ProcessCheckAnswerDtoCommand(
            IGetQuestionQuery getQuestionQuery,
            IGetAnswerQuery getAnswerQuery,
            GetQuestionQuery getQuestionnaireQuery,
            IGetResponseQuery getResponseQuery,
            ICalculateMaturityCommand calculateMaturityCommand)
        {
            _getQuestionQuery = getQuestionQuery;
            _getAnswerQuery = getAnswerQuery;
            _getQuestionnaireQuery = getQuestionnaireQuery;
            _getResponseQuery = getResponseQuery;
            _calculateMaturityCommand = calculateMaturityCommand;
        }

        private async Task<CheckAnswerDto> _GetCheckAnswerDto(int submissionId)
        {
            var responseList = await _GetResponseList(submissionId);

            CheckAnswerDto checkAnswerDto = new CheckAnswerDto();

            Dictionary<string, int> indexMap = new Dictionary<string, int>();
            Dictionary<string, DateTime> dateTimeMap = new Dictionary<string, DateTime>();

            int index = 0;

            foreach (Dfe.PlanTech.Domain.Responses.Models.Response response in responseList ?? throw new NullReferenceException(nameof(responseList)))
            {
                var question = await _GetResponseQuestion(response.QuestionId);
                string questionContentfulRef = question?.ContentfulRef ?? throw new NullReferenceException(nameof(question.ContentfulRef));
                string questionText = question?.QuestionText ?? throw new NullReferenceException(nameof(questionText));

                if (dateTimeMap.ContainsKey(questionContentfulRef))
                {
                    if (DateTime.Compare(response.DateCreated, dateTimeMap[questionContentfulRef]) > 0)
                    {
                        checkAnswerDto.QuestionAnswerList[indexMap[questionContentfulRef]] = await _CreateQuestionWithAnswer(questionContentfulRef, questionText, response.AnswerId);
                    }
                }
                else
                {
                    checkAnswerDto.QuestionAnswerList.Add(await _CreateQuestionWithAnswer(questionContentfulRef, questionText, response.AnswerId));
                    dateTimeMap.Add(questionContentfulRef, response.DateCreated);
                    indexMap.Add(questionContentfulRef, index++);
                }
            }

            return checkAnswerDto;
        }

        private async Task<QuestionWithAnswer> _CreateQuestionWithAnswer(string questionRef, string questionText, int answerId)
        {
            var answer = await _GetResponseAnswer(answerId);
            string answerContentfulRef = answer?.ContentfulRef ?? throw new NullReferenceException(nameof(answer.ContentfulRef));
            string answerText = answer?.AnswerText ?? throw new NullReferenceException(nameof(answerText));

            return new QuestionWithAnswer()
            {
                QuestionRef = questionRef,
                QuestionText = questionText,
                AnswerRef = answerContentfulRef,
                AnswerText = answerText
            };
        }

        private async Task<CheckAnswerDto> _RemoveDetachedQuestions(CheckAnswerDto checkAnswerDto)
        {
            int questionAnswerListCount = checkAnswerDto.QuestionAnswerList.Count;
            if (questionAnswerListCount <= 1) return checkAnswerDto;

            Dictionary<string, bool> isDetachedMap = new Dictionary<string, bool>();

            for (int i = 0; i < questionAnswerListCount; i++) isDetachedMap.Add(checkAnswerDto.QuestionAnswerList[i].QuestionRef, i != 0);

            foreach (QuestionWithAnswer questionWithAnswer in checkAnswerDto.QuestionAnswerList)
            {
                Domain.Questionnaire.Models.Answer answer = await _GetAnswer(questionWithAnswer.QuestionRef, questionWithAnswer.AnswerRef) ?? throw new NullReferenceException(nameof(answer));
                string? nextQuestionId = answer.NextQuestion?.Sys.Id;
                if (nextQuestionId == null) continue;
                if (isDetachedMap.ContainsKey(nextQuestionId)) isDetachedMap[nextQuestionId] = false;
            }

            checkAnswerDto.QuestionAnswerList.RemoveAll(questionWithAnswer => isDetachedMap[questionWithAnswer.QuestionRef]);

            return checkAnswerDto;
        }

        public async Task<CheckAnswerDto> ProcessCheckAnswerDto(int submissionId)
        {
            CheckAnswerDto checkAnswerDto = await _GetCheckAnswerDto(submissionId);
            checkAnswerDto = await _RemoveDetachedQuestions(checkAnswerDto);
            return checkAnswerDto;
        }

        public async Task CalculateMaturityAsync(int submissionId)
        {
            await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);
        }

        private async Task<Domain.Questions.Models.Question?> _GetResponseQuestion(int questionId)
        {
            return await _getQuestionQuery.GetQuestionBy(questionId);
        }

        private async Task<Domain.Answers.Models.Answer?> _GetResponseAnswer(int answerId)
        {
            return await _getAnswerQuery.GetAnswerBy(answerId);
        }

        private async Task<Domain.Questionnaire.Models.Answer?> _GetAnswer(string questionRef, string answerRef)
        {
            if (string.IsNullOrEmpty(questionRef)) throw new ArgumentNullException(nameof(questionRef));
            return (await _getQuestionnaireQuery.GetQuestionById(questionRef, null, CancellationToken.None) ?? throw new KeyNotFoundException($"Could not find answer with id {answerRef}")).Answers.First(answer => answer.Sys.Id.Equals(answerRef));
        }

        private async Task<Domain.Responses.Models.Response[]?> _GetResponseList(int submissionId)
        {
            return await _getResponseQuery.GetResponseListBy(submissionId);
        }
    }
}