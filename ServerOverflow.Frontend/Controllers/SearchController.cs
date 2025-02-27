using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Frontend.Models;
using ServerOverflow.Shared.Storage;
using static System.Int32;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ServerOverflow.Frontend.Controllers;

/// <summary>
/// Database search controller
/// </summary>
[Route("search")]
public class SearchController : Controller {
    [Route("servers")]
    public async Task<IActionResult> Servers() {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");
        if (!account.HasPermission(Permission.SearchServers))
            return Unauthorized();
        
        var model = new SearchModel();
        if (!Request.Query.TryGetValue("q", out var query)) 
            return View(model);
        model.Query = query;
        
        model.CurrentPage = 1;
        if (Request.Query.TryGetValue("page", out var pageStr))
            if (TryParse(pageStr, out var page))
                model.CurrentPage = page;
        if (model.CurrentPage < 1)
            model.CurrentPage = 1;
        
        try {
            var watch = new Stopwatch(); watch.Start();
            var doc = Query.Servers(model.Query!);
            var find = Database.Servers.Find(doc);
            model.TotalMatches = await find.CountDocumentsAsync();
            var elapsedCount = watch.Elapsed;
            
            if (model.TotalMatches == 0) {
                model.Message = "No matches found for your query";
                model.Success = false;
                return View(model);
            }
            
            model.TotalPages = (int)Math.Ceiling(model.TotalMatches / 50f);
            if (model.CurrentPage > model.TotalPages) model.CurrentPage = model.TotalPages;
            watch.Restart();
            using var cursor = await find.Skip(50 * (model.CurrentPage-1)).Limit(50).ToCursorAsync();
            model.Items = []; model.Items.AddRange(cursor.ToList());
            Log.Information("{0} searched for {1} (took {2} to count, {3} to search)",
                account.Username, model.Query, elapsedCount, watch.Elapsed);
        } catch (Exception e) {
            model.Message = e.Message;
            model.Success = false;
        }
        
        return View(model);
    }
    
    [Route("accounts")]
    public async Task<IActionResult> Accounts() {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");
        if (!account.HasPermission(Permission.SearchAccounts))
            return Unauthorized();
        return View();
    }
}
