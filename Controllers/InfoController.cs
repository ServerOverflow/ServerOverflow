using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Database;
using ServerOverflow.Models;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ServerOverflow.Controllers;

/// <summary>
/// Information controller
/// </summary>
public class InfoController : Controller {
    [Route("server/{id}")]
    public async Task<IActionResult> Server(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");

        var server = await Database.Server.Get(id);
        if (server == null) return NotFound();
        if (!account.HasPermission(Permission.SearchServers))
            return Unauthorized();

        return View(new GenericModel<Server> {
            Item = server
        });
    }
}