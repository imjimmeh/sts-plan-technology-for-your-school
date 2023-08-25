﻿using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetSubmissionStatusesQueryTests
    {
        private IPlanTechDbContext mockDb = Substitute.For<IPlanTechDbContext>();

        [Fact]
        public void GetSectionSubmissionStatuses_ReturnsListOfStatuses()
        {
            var expectedStatuses = new List<SectionStatuses>() { new SectionStatuses { Completed = 1, SectionId = "1", Maturity = "Low", DateCreated = DateTime.UtcNow } }.AsQueryable();
            var sections = new Section[1] { new Section { Sys = new Sys { Id = "1" } } };
            mockDb.GetSectionStatuses(Arg.Any<string>()).Returns(expectedStatuses);

            var result = CreateStrut().GetSectionSubmissionStatuses(sections);

            Assert.Equal(result.Count, expectedStatuses.ToList().Count);
        }

        private GetSubmissionStatusesQuery CreateStrut()
        {
            return new GetSubmissionStatusesQuery(mockDb);
        }
    }
}
