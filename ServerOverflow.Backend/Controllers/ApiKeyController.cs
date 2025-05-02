using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

/// <summary>
/// API keys
/// </summary>
[ApiController]
[Route("/api/key")]
public class ApiKeyController : ControllerBase {
    /// <summary>
    /// Retrieves an API key
    /// </summary>
    /// <remarks>API key, if specified, will be ignored. Only cookie authorization is allowed.</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">API key with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved API key</response>
    /// <param name="id">API key ID</param>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(ApiKeyModel), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount(true);
        if (account == null)
            return ValidationProblem(
                title: "Invalid authorization cookie",
                detail: "Failed to retrieve account via cookie auth",
                statusCode: 401);
        
        var target = await ApiKey.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve API key", 
                detail: "API key with specified ID does not exist",
                statusCode: 404);
        
        if (target.Owner != account.Id)
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Retrieving API keys that belong to other users is prohibited",
                statusCode: 403);
        
        return Ok(new ApiKeyModel(target));
    }

    /// <summary>
    /// Modifies an API key
    /// </summary>
    /// <remarks>API key, if specified, will be ignored. Only cookie authorization is allowed.</remarks>
    /// <response code="400">Invalid expiration date or permissions</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">API key with specified ID does not exist</response>
    /// <response code="200">Successfully modified API key</response>
    /// <param name="id">API key ID</param>
    /// <param name="model">You can modify Name, Permissions, ExpireAt</param>
    [HttpPost] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(ApiKeyModel), 200)]
    public async Task<IActionResult> Modify(string id, [FromBody] ApiKeyModel model) {
        var account = await HttpContext.GetAccount(true);
        if (account == null)
            return ValidationProblem(
                title: "Invalid authorization cookie",
                detail: "Failed to retrieve account via cookie auth",
                statusCode: 401);
        
        var target = await ApiKey.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve API key", 
                detail: "API key with specified ID does not exist",
                statusCode: 404);
        
        if (target.Owner != account.Id)
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Modifying API keys that belong to other users is prohibited",
                statusCode: 403);

        if (model.Name != null)
            target.Name = target.Name;

        if (model.Permissions != null) {
            if (!model.Permissions.All(account.Permissions.Contains))
                return ValidationProblem(
                    title: "Invalid permissions list",
                    detail: "You can't grant permissions that you don't have",
                    statusCode: 401);
            
            target.Permissions = model.Permissions;
        }
        
        if (model.ExpireAt != null) {
            if (model.ExpireAt < DateTime.UtcNow + TimeSpan.FromDays(1))
                return ValidationProblem(
                    title: "Invalid expiration date specified",
                    detail: "Minimum API Key lifespan is 1 day");

            target.ExpireAt = model.ExpireAt;
        }
        
        await Audit.UpdatedApiKey(account, target);
        await target.Update();
        return Ok(new ApiKeyModel(target));
    }
    
    /// <summary>
    /// Deletes an API key
    /// </summary>
    /// <remarks>API key, if specified, will be ignored. Only cookie authorization is allowed.</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">API key with specified ID does not exist</response>
    /// <response code="200">Successfully deleted API key</response>
    /// <param name="id">API key ID</param>
    [HttpDelete] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(string id) {
        var account = await HttpContext.GetAccount(true);
        if (account == null)
            return ValidationProblem(
                title: "Invalid authorization cookie",
                detail: "Failed to retrieve account via cookie auth",
                statusCode: 401);
        
        var target = await ApiKey.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve API key", 
                detail: "API key with specified ID does not exist",
                statusCode: 404);
        
        if (target.Owner != account.Id)
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Deleting API keys that belong to other users is prohibited",
                statusCode: 403);

        await Audit.DeletedApiKey(account, target);
        await Database.ApiKeys.Delete(x => x.Id.ToString() == id);
        return Ok();
    }

    /// <summary>
    /// Creates a new API key
    /// </summary>
    /// <remarks>API key, if specified, will be ignored. Only cookie authorization is allowed.</remarks>
    /// <response code="400">Required field is missing or invalid expiration date specified</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="200">Successfully created a new API key</response>
    /// <param name="model">You must specify Name, Permissions, ExpireAt</param>
    [HttpPost] [Route("create")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ApiKeyModel), 200)]
    public async Task<IActionResult> Create([FromBody] ApiKeyModel model) {
        var account = await HttpContext.GetAccount(true);
        if (account == null)
            return ValidationProblem(
                title: "Invalid authorization cookie",
                detail: "Failed to retrieve account via cookie auth",
                statusCode: 401);
        
        if (model.Name == null || model.Permissions == null || model.ExpireAt == null)
            return ValidationProblem(
                title: "Missing required fields", 
                detail: "API key name, permissions, and expiration date are required");
        
        if (model.ExpireAt < DateTime.UtcNow + TimeSpan.FromDays(1))
            return ValidationProblem(
                title: "Invalid expiration date specified",
                detail: "Minimum API Key lifespan is 1 day");
        
        var target = await ApiKey.Create(account, model.Name, model.Permissions, model.ExpireAt.Value);
        await Audit.CreatedApiKey(account, target);
        return Ok(new ApiKeyModel(target, true));
    }
    
    /// <summary>
    /// Lists all API keys owned by current account
    /// </summary>
    /// <remarks>API key, if specified, will be ignored. Only cookie authorization is allowed.</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="200">Successfully retrieved all API keys</response>
    [HttpGet] [Route("list")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(List<ApiKeyModel>), 200)]
    public async Task<IActionResult> List() {
        var account = await HttpContext.GetAccount(true);
        if (account == null)
            return ValidationProblem(
                title: "Invalid authorization cookie",
                detail: "Failed to retrieve account via cookie auth",
                statusCode: 401);;
        
        // TODO: maybe add pagination later on?
        var keys = await Database.ApiKeys.QueryAll(x => x.Owner == account.Id);
        return Ok(keys.Select(x => new ApiKeyModel(x)).ToList());
    }
}