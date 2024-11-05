using Microsoft.AspNetCore.Mvc;

namespace ServerOverflow.Controllers;

public class HomeController : Controller {
    [Route("")]
    public IActionResult Index() => View();
    
    [Route("faq")]
    public IActionResult FAQ() => View();
    
    [Route("error")]
    public IActionResult Error() => View();
}