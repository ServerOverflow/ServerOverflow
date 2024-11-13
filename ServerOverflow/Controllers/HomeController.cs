using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Models;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ServerOverflow.Controllers;

/// <summary>
/// Home pages controller
/// </summary>
public class HomeController : Controller {
    [Route("")]
    public IActionResult Index() => View();
    
    [Route("faq")]
    public IActionResult FAQ() => View();

    [Route("stats")]
    public IActionResult Stats() => View();
    
    [Route("stats.json")]
    public IActionResult StatsDownload() => File(System.IO.File.ReadAllBytes("stats.json"), "application/json");
    
    [Route("error")]
    public IActionResult Error() => View(new ErrorModel { StatusCode = Response.StatusCode });
}