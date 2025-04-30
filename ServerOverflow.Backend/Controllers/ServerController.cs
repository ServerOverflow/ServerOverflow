using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

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
    /// Searches for a server using query language
    /// </summary>
    /// <remarks>Search Servers permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="202">Zero results for specified query</response>
    /// <response code="200">Successfully retrieved servers</response>
    [HttpPost] [Route("search")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(202)]
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
        
        var doc = Query.Servers(model.Query!) & Builders<Server>.Filter.Ne(x => x.Id, ObjectId.Empty);
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
        
        await Audit.SearchedServers(account, model.Query, model.CurrentPage, model.TotalPages, model.TotalMatches);
        return Ok(model);
    }
}