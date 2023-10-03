using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

/// <summary>
/// A subsection of a <see chref="ICategory"/>
/// </summary>
public interface ISection : IContentComponent
{
    public string Name { get; }

    public Question[] Questions { get; }

    public string FirstQuestionId { get; }

    public Page InterstitialPage { get; }

    public RecommendationPage[] Recommendations { get; }

    public RecommendationPage? TryGetRecommendationForMaturity(Maturity maturity);

    public RecommendationPage? GetRecommendationForMaturity(string? maturity);

    public IEnumerable<QuestionWithAnswer> GetAttachedQuestions(IEnumerable<QuestionWithAnswer> responses);
}
