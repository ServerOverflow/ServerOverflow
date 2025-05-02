using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

/// <summary>
/// Audit logs
/// </summary>
[ApiController]
[Route("/api/audit")]
public class AuditController : ControllerBase {
    /// <summary>
    /// Retrieves an audit log entry
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="404">Log entry with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved log entry</response>
    /// <param name="id">Log entry ID</param>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(LogEntry), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);

        var target = await LogEntry.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve log entry", 
                detail: "Log entry with specified ID does not exist",
                statusCode: 404);
        
        return Ok(target);
    }

    /// <summary>
    /// Searches for audit log entries
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="204">Zero results for specified query</response>
    /// <response code="200">Successfully retrieved log entries</response>
    /// <param name="query">Query with operators</param>
    /// <param name="page">Page number from 1</param>
    /// <param name="startId">Log entry ID to start pagination from</param>
    [HttpPost] [Route("search")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(PaginationModel<LogEntry>), 200)]
    public async Task<IActionResult> Search([FromQuery] string query = "", [FromQuery] int page = 1, [FromQuery] string? startId = null) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        var model = new PaginationModel<LogEntry> {
            CurrentPage = page, Query = query
        };

        if (model.CurrentPage < 1)
            model.CurrentPage = 1;

        try {
            var doc = Query.LogEntry(model.Query!);
            if (startId != null)
                doc &= Builders<LogEntry>.Filter.Lte(x => x.Id, new ObjectId(startId));
            var sort = Builders<LogEntry>.Sort.Descending(x => x.Id); 
            var find = Database.AuditLog.Find(doc).Sort(sort);
            model.TotalMatches = await find.CountAsync();
            if (model.TotalMatches == 0) return NoContent();
            
            model.TotalPages = (int)Math.Ceiling(model.TotalMatches / 50f);
            if (model.CurrentPage > model.TotalPages)
                model.CurrentPage = model.TotalPages;
        
            using var cursor = await find.Skip(50 * (model.CurrentPage-1)).Limit(50).ToCursorAsync();
            model.Items = []; model.Items.AddRange(cursor.ToList());
            return Ok(model);
        } catch (Exception e) {
            return ValidationProblem(
                title: e.GetType().Name, 
                detail: e.Message);
        }
    }
}