﻿using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries
{
    public class GetSubmissionStatusesQueryTests
    {
        private IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();
        private IUser user = Substitute.For<IUser>();

        

        [Fact]
        public void GetSectionSubmissionStatuses_ReturnsListOfStatuses()
        {
            var expectedStatuses = new List<SectionStatuses>() { new SectionStatuses { Completed = 1, SectionId = "1", Maturity = "Low", DateCreated = DateTime.UtcNow } }.AsQueryable();
            var sections = new Section[1] { new Section { Sys = new SystemDetails { Id = "1" } } };
            Db.GetSectionStatuses(Arg.Any<string>(), Arg.Any<int>()).Returns(expectedStatuses);

            var result = CreateStrut().GetSectionSubmissionStatuses(sections);

            Assert.Equal(result.Count, expectedStatuses.ToList().Count);
        }

        private GetSubmissionStatusesQuery CreateStrut() => new GetSubmissionStatusesQuery(Db, user);
    }
}
