using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MineProtocol.Authentication;
using MineProtocol.Schema;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;
using Profile = ServerOverflow.Shared.Storage.Profile;

namespace ServerOverflow.Backend.Controllers;

[ApiController]
[Route("/api/profile")]
public class ProfileController : ControllerBase {
    /// <summary>
    /// Retrieves a minecraft profile
    /// </summary>
    /// <remarks>Administrator permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="404">Profile with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved minecraft profile</response>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(Profile), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);

        var target = await Profile.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve profile", 
                detail: "Profile with specified ID does not exist",
                statusCode: 404);
        
        return Ok(new ProfileModel(target));
    }
    
    /// <summary>
    /// Deletes a minecraft profile
    /// </summary>
    /// <remarks>Administrator permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Profile with specified ID does not exist</response>
    /// <response code="200">Successfully deleted minecraft profile</response>
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
                detail: "Retrieving profiles requires Administrator permission",
                statusCode: 403);

        var target = await Profile.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve profile", 
                detail: "Profile with specified ID does not exist",
                statusCode: 404);

        await Database.Profiles.Delete(x => x.Id.ToString() == id);
        await Audit.DeletedProfile(account, target);
        return Ok();
    }
    
    /// <summary>
    /// Initiates OAuth device authentication
    /// </summary>
    /// <response code="200">Successfully initiated the OAuth device flow</response>
    [HttpGet] [Route("add")]
    [ProducesResponseType(typeof(DeviceAuthResponse), 200)]
    public async Task<IActionResult> Code()
        => Ok(await OAuth2.DeviceCode());

    /// <summary>
    /// Polls OAuth device code and adds the minecraft profile when finished
    /// </summary>
    /// <response code="400">Account does not own minecraft</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="404">Device code has expired or is not valid</response>
    /// <response code="202">Polling is still in progress</response>
    /// <response code="200">Successfully added minecraft profile</response>
    [HttpGet] [Route("poll/{code}")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(204)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Poll(string code) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        TokenPair? token;
        try {
            token = await OAuth2.PollToken(code);
        } catch {
            return ValidationProblem(
                title: "Failed to poll device token", 
                detail: "Device code has expired or is not valid",
                statusCode: 404);
        }

        if (token == null)
            return NoContent();
        
        try {
            var profile = new Profile {
                Instance = new MineProtocol.Authentication.Profile(token),
                Valid = true
            };
            
            await Database.Profiles.InsertOneAsync(profile);
            await Audit.CreatedProfile(account, profile);
            return Ok();
        } catch {
            return ValidationProblem(
                title: "Failed to poll device token", 
                detail: "Account does not own minecraft",
                statusCode: 400);
        }
    }
    
    /// <summary>
    /// Lists all minecraft profiles
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="200">Successfully retrieved all profiles</response>
    [HttpGet] [Route("list")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(List<ProfileModel>), 200)]
    public async Task<IActionResult> List() {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        // TODO: maybe add pagination later on?
        var accounts = await Database.Profiles.QueryAll(x => true);
        return Ok(accounts.Select(x => new ProfileModel(x)).ToList());
    }
}