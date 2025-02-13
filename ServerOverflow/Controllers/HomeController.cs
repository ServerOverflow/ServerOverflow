using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Models;
using ServerOverflow.Storage;
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
    public IActionResult StatsDownload()
        => File(System.IO.File.OpenRead("stats.json"), "application/json");
    
    [Route("error")]
    public IActionResult Error() => View(new ErrorModel { StatusCode = Response.StatusCode });
    
    [Route("favicon/{id}")]
    public async Task<IActionResult> Favicon(string id) {
        var server = await Server.Get(id);
        if (server == null) 
            return Redirect("/img/default.png");
        var enc = server.Ping.Favicon;
        if (enc == null || string.IsNullOrWhiteSpace(enc) || !enc.StartsWith("data:image"))
            return Redirect("/img/default.png");
        
        var parts = enc.Split(',');
        if (parts.Length != 2) 
            return Redirect("/img/default.png");

        var type = parts[0].Split(':')[1].Split(';')[0];
        var base64 = parts[1];

        try {
            var imageBytes = Convert.FromBase64String(base64);
            return File(imageBytes, type);
        } catch (FormatException) {
            return Redirect("/img/default.png");
        }
    }
}