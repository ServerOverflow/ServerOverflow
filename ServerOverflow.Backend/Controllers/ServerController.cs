using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

/// <summary>
/// Minecraft servers
/// </summary>
[ApiController]
[Route("/api/server")]
public class ServerController : ControllerBase {
    /// <summary>
    /// Retrieves a server
    /// </summary>
    /// <remarks>Search Servers permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Server with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved server</response>
    /// <param name="id">Server ID</param>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(Server), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.SearchServers))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Retrieving servers requires Search Servers permission",
                statusCode: 403);

        var target = await Server.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve server", 
                detail: "Server with specified ID does not exist",
                statusCode: 404);
        
        return Ok(target);
    }
    
    /// <summary>
    /// Retrieves a server's favicon
    /// </summary>
    /// <response code="200">Successfully retrieved the favicon</response>
    /// <response code="302">Failed to decode favicon or unknown server</response>
    /// <param name="id">Server ID</param>
    [HttpGet] [Route("{id}.png")]
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
    
    /// <summary>
    /// Retrieves basic server statistics
    /// </summary>
    /// <response code="200">Successfully retrieved statistics</response>
    [HttpGet] [Route("stats")]
    [ProducesResponseType(typeof(StatisticsModel), 200)]
    public IActionResult Statistics()
        => Ok(new StatisticsModel {
            NotConfiguredServers = (int)Services.Statistics.Servers.WithLabels("not_configured").Value,
            OnlineServers = (int)Services.Statistics.Servers.WithLabels("online").Value,
            TotalServers = (int)Services.Statistics.Servers.WithLabels("total").Value
        });

    /// <summary>
    /// Searches for a server using query language
    /// </summary>
    /// <remarks>Search Servers permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="204">Zero results for specified query</response>
    /// <response code="200">Successfully retrieved servers</response>
    /// <param name="query">Query with operators</param>
    /// <param name="page">Page number from 1</param>
    [HttpPost] [Route("search")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(PaginationModel<Server>), 200)]
    public async Task<IActionResult> Search([FromQuery] string query = "", [FromQuery] int page = 1) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.SearchServers))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Retrieving servers requires Search Servers permission",
                statusCode: 403);
        
        var model = new PaginationModel<Server> {
            CurrentPage = page, Query = query
        };

        if (model.CurrentPage < 1)
            model.CurrentPage = 1;

        try {
            var watch = new Stopwatch(); watch.Start();
            var doc = Query.Server(model.Query!);
            var find = Database.Servers.Find(doc);
            model.TotalMatches = await find.CountAsync();
            if (model.TotalMatches == 0) {
                await Audit.SearchedServers(account, model.Query);
                return NoContent();
            }
            
            model.TotalPages = (int)Math.Ceiling(model.TotalMatches / 50f);
            if (model.CurrentPage > model.TotalPages)
                model.CurrentPage = model.TotalPages;
        
            using var cursor = await find.Skip(50 * (model.CurrentPage-1)).Limit(50).ToCursorAsync();
            model.Items = []; model.Items.AddRange(cursor.ToList());
            watch.Stop(); model.Milliseconds = watch.ElapsedMilliseconds;
        
            await Audit.SearchedServers(account, model.Query, model.CurrentPage, model.TotalPages, model.TotalMatches);
            return Ok(model);
        } catch (Exception e) {
            return ValidationProblem(
                title: e.GetType().Name, 
                detail: e.Message);
        }
    }
    
    
}