using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models;

public class PageViewModel : BaseViewModel
{
    public required Page Page { get; init; }
}