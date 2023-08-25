using Microsoft.Extensions.Logging;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models
{
    public class Category : ContentComponent, ICategory
    {
        private readonly IGetSubmissionStatusesQuery _query;
        private readonly ILogger<Category> _logger;

        public Header Header { get; set; } = null!;
        public IContentComponent[] Content { get; set; } = Array.Empty<IContentComponent>();
        public ISection[] Sections { get; set; } = Array.Empty<ISection>();
        public IList<SectionStatuses> SectionStatuses { get; set; } = new List<SectionStatuses>();
        public int Completed { get; set; }
        public bool RetrievalError { get; set; }

        public Category(ILogger<Category> logger, IGetSubmissionStatusesQuery Query){
            _logger = logger;
            _query = Query;
        }

        public void RetrieveSectionStatuses()
        {
            try
            {
                SectionStatuses = _query.GetSectionSubmissionStatuses(Sections).ToList();
                Completed = SectionStatuses.Count(x => x.Completed == 1);
                RetrievalError = false;
            }
            catch (Exception e)
            {
                _logger.LogError("An exception has occurred while trying to retrieve section progress with the following message - {}", e.Message);
                RetrievalError = true;
            }
        }
    }
}