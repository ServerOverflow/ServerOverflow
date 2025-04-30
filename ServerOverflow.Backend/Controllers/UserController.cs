using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

[ApiController]
[Route("/api/user")]
public class UserController : ControllerBase {
    /// <summary>
    /// Retrieves a users account info
    /// </summary>
    /// <remarks>Search Accounts permission is required to retrieve info about other accounts</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Account with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved users account</response>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(UserModel), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (id == "me") id = account.Id.ToString();
        if (id != account.Id.ToString() && !account.HasPermission(Permission.SearchAccounts))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Retrieving other users accounts requires Search Accounts permission",
                statusCode: 403);

        var target = await Account.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve user account", 
                detail: "Account with specified ID does not exist",
                statusCode: 404);
        
        return Ok(new UserModel(target));
    }
    
    /// <summary>
    /// Modifies a users account
    /// </summary>
    /// <remarks>Administrator permission is required to modify other users or to modify granted permissions</remarks>
    /// <response code="400">Username has already been taken</response>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Account with specified ID does not exist</response>
    /// <response code="200">Successfully modified users account</response>
    [HttpPost] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(UserModel), 200)]
    public async Task<IActionResult> Modify(string id, [FromBody] UserModel model) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (id == "me") id = account.Id.ToString();
        if (id != account.Id.ToString() && !account.HasPermission(Permission.Administrator))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Modifying other users accounts requires Administrator permission",
                statusCode: 403);
        
        var target = await Account.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve user account", 
                detail: "Account with specified ID does not exist",
                statusCode: 404);

        if (model.NewPassword != null)
            target.Password = model.NewPassword.GetHash();

        if (model.Permissions != null) {
            if (!account.HasPermission(Permission.Administrator))
                return ValidationProblem(
                    title: "Required permission was not granted", 
                    detail: "Modifying granted permissions requires Administrator permission",
                    statusCode: 403);
            
            target.Permissions = model.Permissions;
        }
        
        if (model.Username != null) {
            if (await Account.GetByName(model.Username) != null)
                return ValidationProblem(
                    title: "Invalid username specified",
                    detail: "This username has already been taken");
            
            target.Username = model.Username;
        }
        
        await Audit.UpdatedAccount(account, target);
        await target.Update();
        return Ok(new UserModel(target));
    }
    
    /// <summary>
    /// Deletes a users account
    /// </summary>
    /// <remarks>Administrator permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="404">Account with specified ID does not exist</response>
    /// <response code="200">Successfully deleted users account</response>
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
                detail: "Deleting users account requires Administrator permission",
                statusCode: 403);

        var target = await Account.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve user account", 
                detail: "Account with specified ID does not exist",
                statusCode: 404);

        await Database.Accounts.Delete(x => x.Id.ToString() == id);
        await Audit.DeletedAccount(account, target);
        return Ok();
    }
    
    /// <summary>
    /// Creates a new account with an invitation code
    /// </summary>
    /// <response code="400">Invalid invitation code or username has been taken</response>
    /// <response code="200">Account crated successfully</response>
    [HttpPost] [Route("register")]
    [ProducesResponseType(typeof(ValidationProblem), 400)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model) {
        var invite = await Invitation.GetByCode(model.InviteCode);
        if (invite == null)
            return ValidationProblem(
                title: "Invalid invitation code",
                detail: "This invitation code does not exist");
        
        if (invite.Used)
            return ValidationProblem(
                title: "Invalid invitation code",
                detail: "This invitation code has already been used");

        if (await Account.GetByName(model.Username) != null)
            return ValidationProblem(
                title: "Invalid username specified",
                detail: "This username has already been taken");

        var account = await Account.Create(model.Username, model.Password, invite);
        await Audit.Registered(account);
        await HttpContext.SignIn(account);
        return Ok(new UserModel(account));
    }
    
    /// <summary>
    /// Logs into an account by username and password
    /// </summary>
    /// <response code="401">Invalid username or password</response>
    /// <response code="200">Successfully logged in</response>
    [HttpPost] [Route("login")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Login([FromBody] LoginModel model) {
        var account = await Account.Get(model.Username, model.Password);
        if (account == null)
            return ValidationProblem(
                title: "Wrong username or password",
                detail: "Failed to retrieve account with specified credentials",
                statusCode: 401);
        
        await Audit.LoggedIn(account);
        await HttpContext.SignIn(account);
        return Ok(new UserModel(account));
    }
    
    /// <summary>
    /// Lists all user accounts
    /// </summary>
    /// <remarks>Search Accounts permission is required</remarks>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="200">Successfully retrieved all users accounts</response>
    [HttpGet] [Route("list")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(List<UserModel>), 200)]
    public async Task<IActionResult> List() {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        if (!account.HasPermission(Permission.SearchAccounts))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Listing all user accounts requires Search Accounts permission",
                statusCode: 403);
        
        // TODO: maybe add pagination later on?
        var accounts = await Database.Accounts.QueryAll(x => true);
        return Ok(accounts.Select(x => new UserModel(x)).ToList());
    }
    
    /// <summary>
    /// Logs out of the current account
    /// </summary>
    /// <response code="200">Successfully logged out</response>
    [HttpGet] [Route("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}