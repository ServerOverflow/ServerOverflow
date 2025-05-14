using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

/// <summary>
/// IP range exclusions
/// </summary>
[ApiController]
[Route("/api/exclusion")]
public class ExclusionController : ControllerBase {
    /// <summary>
    /// Retrieves an exclusion
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Exclusion with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved exclusion</response>
    /// <param name="id">Exclusion ID</param>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(ExclusionModel), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);

        var target = await Exclusion.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve exclusion", 
                detail: "Exclusion with specified ID does not exist",
                statusCode: 404);
        
        return Ok(new ExclusionModel(target));
    }

    /// <summary>
    /// Modifies an exclusion
    /// </summary>
    /// <remarks>Modify Exclusions permission is required</remarks>
    /// <response code="400">Invalid expiration date or permissions</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Exclusion with specified ID does not exist</response>
    /// <response code="200">Successfully modified exclusion</response>
    /// <param name="id">Exclusion ID</param>
    /// <param name="model">You can modify Comment, Ranges</param>
    [HttpPost] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(ExclusionModel), 200)]
    public async Task<IActionResult> Modify(string id, [FromBody] ExclusionModel model) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.ModifyExclusions))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Modifying exclusions requires Modify Exclusions permission",
                statusCode: 403);
        
        var target = await Exclusion.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve exclusion", 
                detail: "Exclusion with specified ID does not exist",
                statusCode: 404);

        if (model.Comment != null)
            target.Comment = model.Comment;

        if (model.Ranges != null)
            target.Ranges = model.Ranges;
        
        await Audit.UpdatedExclusion(account, target);
        await target.Update();
        return Ok(new ExclusionModel(target));
    }
    
    /// <summary>
    /// Deletes an exclusion
    /// </summary>
    /// <remarks>Modify Exclusions permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Exclusion with specified ID does not exist</response>
    /// <response code="200">Successfully deleted exclusion</response>
    /// <param name="id">Exclusion ID</param>
    [HttpDelete] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.ModifyExclusions))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Deleting exclusions requires Modify Exclusions permission",
                statusCode: 403);
        
        var target = await Exclusion.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve exclusion", 
                detail: "Exclusion with specified ID does not exist",
                statusCode: 404);

        await Audit.DeletedExclusion(account, target);
        await Database.Exclusions.Delete(x => x.Id.ToString() == id);
        return Ok();
    }

    /// <summary>
    /// Creates a new exclusion
    /// </summary>
    /// <remarks>Modify Exclusions permission is required</remarks>
    /// <response code="400">Required field is missing or invalid expiration date specified</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="200">Successfully created a new exclusion</response>
    /// <param name="model">You must specify Comment, Ranges</param>
    [HttpPost] [Route("create")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ExclusionModel), 200)]
    public async Task<IActionResult> Create([FromBody] ExclusionModel model) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.ModifyExclusions))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Creating exclusions requires Modify Exclusions permission",
                statusCode: 403);

        if (model.Comment == null || model.Ranges == null)
            return ValidationProblem(
                title: "Missing required fields", 
                detail: "Exclusion comment and ranges are required");

        var target = await Exclusion.Create(model.Ranges.ToArray(), model.Comment);
        await Audit.CreatedExclusion(account, target);
        return Ok(new ExclusionModel(target));
    }
    
    /// <summary>
    /// Searches for an exclusion using query language
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="204">Zero results for specified query</response>
    /// <response code="200">Successfully retrieved exclusions</response>
    /// <param name="query">Query with operators</param>
    /// <param name="page">Page number from 1</param>
    [HttpPost] [Route("search")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(PaginationModel<ExclusionModel>), 200)]
    public async Task<IActionResult> Search([FromQuery] string query = "", [FromQuery] int page = 1) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        var model = new PaginationModel<ExclusionModel> {
            CurrentPage = page, Query = query
        };

        if (model.CurrentPage < 1)
            model.CurrentPage = 1;
        
        try {
            var doc = Query.Exclusion(model.Query!);
            var find = Database.Exclusions.Find(doc);
            model.TotalMatches = await find.CountAsync();
            if (model.TotalMatches == 0) return NoContent();
            
            model.TotalPages = (int)Math.Ceiling(model.TotalMatches / 25f);
            if (model.CurrentPage > model.TotalPages)
                model.CurrentPage = model.TotalPages;
        
            using var cursor = await find.Skip(25 * (model.CurrentPage-1)).Limit(25).ToCursorAsync();
            model.Items = []; model.Items.AddRange(cursor.ToList().Select(x => new ExclusionModel(x)));
            return Ok(model);
        } catch (Exception e) {
            return ValidationProblem(
                title: e.GetType().Name, 
                detail: e.Message);
        }
    }
}