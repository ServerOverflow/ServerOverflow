using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Frontend.Models;
using ServerOverflow.Shared.Storage;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ServerOverflow.Frontend.Controllers;

/// <summary>
/// Home pages controller
/// </summary>
public class HomeController : Controller {
    [Route("")]
    public IActionResult Index() => View();
    
    [Route("faq")]
    public IActionResult FAQ() => View();
    
    [Route("error")]
    public IActionResult Error() => View(new ErrorModel { StatusCode = Response.StatusCode });
    
    [Route("favicon/{id}")]
    public async Task<IActionResult> Favicon(string id) {
        var server = await Server.Get(id);
        if (server == null) 
            return File(System.IO.File.OpenRead("/img/default.png"), "image/png");
        var enc = server.Ping.Favicon;
        if (enc == null || string.IsNullOrWhiteSpace(enc) || !enc.StartsWith("data:image"))
            return File(System.IO.File.OpenRead("/img/default.png"), "image/png");
        
        var parts = enc.Split(',');
        if (parts.Length != 2) 
            return File(System.IO.File.OpenRead("/img/default.png"), "image/png");

        var type = parts[0].Split(':')[1].Split(';')[0];
        var base64 = parts[1];

        try {
            var imageBytes = Convert.FromBase64String(base64);
            return File(imageBytes, type);
        } catch (FormatException) {
            return File(System.IO.File.OpenRead("/img/default.png"), "image/png");
        }
    }
}