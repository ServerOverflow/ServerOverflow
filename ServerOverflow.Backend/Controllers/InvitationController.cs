using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

/// <summary>
/// Invitation codes
/// </summary>
[ApiController]
[Route("/api/invitation")]
public class InvitationController : ControllerBase {
    /// <summary>
    /// Retrieves an invitation code
    /// </summary>
    /// <remarks>Administrator permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Invitation code with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved invitation code</response>
    /// <param name="id">Invitation ID</param>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(InvitationModel), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.Administrator))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Retrieving invitation codes requires Administrator permission",
                statusCode: 403);

        var target = await Invitation.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve invitation code", 
                detail: "Invitation code with specified ID does not exist",
                statusCode: 404);
        
        return Ok(new InvitationModel(target));
    }

    /// <summary>
    /// Modifies an invitation code
    /// </summary>
    /// <remarks>Administrator permission is required</remarks>
    /// <response code="400">Invitation code has already been taken</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Invitation code with specified ID does not exist</response>
    /// <response code="200">Successfully modified invitation code</response>
    /// <param name="id">Invitation ID</param>
    /// <param name="model">You can modify BadgeText, Used, Code</param>
    [HttpPost] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(InvitationModel), 200)]
    public async Task<IActionResult> Modify(string id, [FromBody] InvitationModel model) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.Administrator))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Modifying invitation codes requires Administrator permission",
                statusCode: 403);
        
        var target = await Invitation.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve invitation code", 
                detail: "Invitation code with specified ID does not exist",
                statusCode: 404);

        if (model.BadgeText != null)
            target.BadgeText = model.BadgeText;

        if (model.Used != null)
            target.Used = model.Used.Value;
        
        if (model.Code != null) {
            if (await Invitation.GetByCode(model.Code) != null)
                return ValidationProblem(
                    title: "Invalid invitation code", 
                    detail: "This invitation code has already been taken",
                    statusCode: 400);
            
            target.Code = model.Code;
        }
        
        await Audit.UpdatedInvitation(account, target);
        await target.Update();
        return Ok(new InvitationModel(target));
    }
    
    /// <summary>
    /// Deletes an invitation code
    /// </summary>
    /// <remarks>Administrator permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Invitation code with specified ID does not exist</response>
    /// <response code="200">Successfully deleted invitation code</response>
    /// <param name="id">Invitation ID</param>
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
        
        if (!account.HasPermission(Permission.Administrator))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Deleting invitation codes requires Administrator permission",
                statusCode: 403);

        var target = await Invitation.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve invitation code", 
                detail: "Invitation code with specified ID does not exist",
                statusCode: 404);

        await Database.Invitations.Delete(x => x.Id.ToString() == id);
        await Audit.DeletedInvitation(account, target);
        return Ok();
    }

    /// <summary>
    /// Creates a new invitation code
    /// </summary>
    /// <response code="400">Required field is missing or invitation code has already been taken</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="200">Successfully created an invitation code</response>
    /// <param name="model">You must specify BadgeText. Code is optional.</param>
    [HttpPost] [Route("create")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(InvitationModel), 200)]
    public async Task<IActionResult> Create([FromBody] InvitationModel model) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.Administrator))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Creating new invitation codes requires Administrator permission",
                statusCode: 403);
        
        if (model.BadgeText == null)
            return ValidationProblem(
                title: "Required field is missing", 
                detail: "Badge text is required to create an invitation code",
                statusCode: 400);
        
        if (model.Code != null && await Invitation.GetByCode(model.Code) != null)
            return ValidationProblem(
                title: "Invalid invitation code", 
                detail: "This invitation code has already been taken",
                statusCode: 400);
        
        var target = await Invitation.Create(model.BadgeText, model.Code, account);
        await Audit.CreatedInvitation(account, target);
        return Ok(new InvitationModel(target));
    }
    
    /// <summary>
    /// Lists all invitation codes
    /// </summary>
    /// <remarks>Administrator permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="200">Successfully retrieved all invitation codes</response>
    [HttpGet] [Route("list")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(List<InvitationModel>), 200)]
    public async Task<IActionResult> List() {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.Administrator))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Listing all invitation codes requires Administrator permission",
                statusCode: 403);
        
        // TODO: maybe add pagination later on?
        var accounts = await Database.Invitations.QueryAll(x => true);
        return Ok(accounts.Select(x => new InvitationModel(x)).ToList());
    }
}