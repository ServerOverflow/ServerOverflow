using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Database;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ServerOverflow.Controllers;

/// <summary>
/// Database search controller
/// </summary>
[Route("search")]
public class SearchController : Controller {
    [Route("accounts")]
    public async Task<IActionResult> Accounts() {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");
        if (!account.HasPermission(Permission.SearchAccounts))
            return Unauthorized();
        return View();
    }
}
