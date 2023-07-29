using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ILogger<TConcreteController> logger;
    protected readonly IUrlHistory history;

    public BaseController(ILogger<TConcreteController> logger, IUrlHistory history)
    {
        this.logger = logger;
        this.history = history;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/Error")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}