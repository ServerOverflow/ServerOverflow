using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MineProtocol.Authentication;
using Serilog;
using ServerOverflow.Database;
using ServerOverflow.Models;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using Profile = ServerOverflow.Database.Profile;

namespace ServerOverflow.Controllers;

/// <summary>
/// User management controller
/// </summary>
[Route("user")]
public class UserController : Controller {
    [Route("register")]
    public async Task<IActionResult> Register() {
        var account = await HttpContext.GetAccount();
        if (account != null) return Redirect("/user/profile");
        
        var model = new StatusModel();
        if (!HttpContext.Request.HasFormContentType
            || !HttpContext.Request.Form.TryGetValue(
                "invite", out var code)
            || !HttpContext.Request.Form.TryGetValue(
                "username", out var username)
            || !HttpContext.Request.Form.TryGetValue(
                "password", out var password))
            return View(model);
        
        var invite = await Invitation.Get(code!);
        if (invite == null) {
            model.Message = "This invitation code does not exist";
            return View(model);
        }
        
        if (invite.Used) {
            model.Message = "This invitation code has already been used";
            return View(model);
        }

        if (await Account.GetByName(username!) != null) {
            model.Message = "This username has already been taken";
            return View(model);
        }

        account = await Account.Create(username!, password!, invite);
        await HttpContext.SignIn(account);
        return Redirect("/user/profile");
    }

    [Route("login")]
    public async Task<IActionResult> Login() {
        var account = await HttpContext.GetAccount();
        if (account != null) return Redirect("/user/profile");
        
        var model = new StatusModel();
        if (!HttpContext.Request.HasFormContentType
            || !HttpContext.Request.Form.TryGetValue(
                "username", out var username)
            || !HttpContext.Request.Form.TryGetValue(
                "password", out var password))
            return View(model);

        account = await Account.Get(username!, password!);
        if (account == null) {
            model.Message = "Wrong username or password";
            return View(model);
        }
        
        await HttpContext.SignIn(account);
        return Redirect("/user/profile");
    }

    [Route("manage")]
    public async Task<IActionResult> Manage() {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");
        var model = new AccountModel {
            Account = account,
            Target = account
        };
        
        if (!HttpContext.Request.HasFormContentType
            || !HttpContext.Request.Form.TryGetValue(
                "type", out var type))
            return View(model);
        
        switch (type.ToString()) {
            case "deleteInvitation": { // Delete Invitation
                if (!account.HasPermission(Permission.Administrator)) {
                    model.Message = "Only administrators can delete invitation codes!";
                    break;
                }
                
                if (!HttpContext.Request.Form.TryGetValue(
                        "id", out var id))
                    break;

                await Database.Database.Invitations.Delete(x => x.Id.ToString() == id.ToString());
                model.Message = "Successfully deleted the invitation!";
                model.Success = true;
                break;
            }
            case "createInvitation": { // Create Invitation
                if (!account.HasPermission(Permission.Administrator)) {
                    model.Message = "Only administrators can create invitation codes!";
                    break;
                }
                
                if (!HttpContext.Request.Form.TryGetValue(
                        "badge", out var badge))
                    break;

                var invitation = await Invitation.Create(badge!, account);
                model.Message = $"Here's your invitation code: <code>{invitation.Code}</code>";
                model.Success = true;
                break;
            }
            case "deleteExclusion": { // Delete Exclusion
                if (!account.HasPermission(Permission.ModifyExclusions)) {
                    model.Message = "Modify exclusions permission is required to delete exclusions!";
                    break;
                }
                
                if (!HttpContext.Request.Form.TryGetValue(
                        "id", out var id))
                    break;

                await Database.Database.Exclusions.Delete(x => x.Id.ToString() == id.ToString());
                model.Message = "Successfully deleted the exclusion!";
                model.Success = true;
                break;
            }
            case "createExclusion": { // Create Exclusion
                if (!account.HasPermission(Permission.ModifyExclusions)) {
                    model.Message = "Modify exclusions permission is required to create exclusions!";
                    break;
                }
                
                if (!HttpContext.Request.Form.TryGetValue(
                        "ranges", out var ranges))
                    break;
                if (!HttpContext.Request.Form.TryGetValue(
                        "comment", out var comment))
                    break;

                await Exclusion.Create(ranges.ToString().Split("\n"), comment!);
                model.Message = "Successfully created a new exclusion!";
                model.Success = true;
                break;
            }
            case "modifyExclusion": { // Modify Exclusion
                if (!account.HasPermission(Permission.ModifyExclusions)) {
                    model.Message = "Modify exclusions permission is required to modify exclusions!";
                    break;
                }
                
                if (!HttpContext.Request.Form.TryGetValue(
                        "id", out var id))
                    break;
                if (!HttpContext.Request.Form.TryGetValue(
                        "ranges", out var ranges))
                    break;
                if (!HttpContext.Request.Form.TryGetValue(
                        "comment", out var comment))
                    break;

                var exclusion = await Exclusion.Get(id!);
                if (exclusion == null) {
                    model.Message = "This exclusion no longer exists!";
                    break;
                }

                exclusion.Ranges = ranges.ToString().Split("\n").ToList();
                exclusion.Comment = comment!; await exclusion.Update();
                model.Message = "Successfully modified that exclusion!";
                model.Success = true;
                break;
            }
            case "deleteAccount": { // Delete account
                if (!account.HasPermission(Permission.Administrator)) {
                    model.Message = "Only administrators can delete accounts!";
                    break;
                }
                
                if (!HttpContext.Request.Form.TryGetValue(
                        "uuid", out var id))
                    break;

                await Database.Database.Profiles.Delete(x => x.Instance.UUID == id.ToString());
                model.Message = "Successfully deleted the account!";
                model.Success = true;
                break;
            }
        }
        
        return View(model);
    }

    [Route("profile")]
    public async Task<IActionResult> Profile() {
        var account = await HttpContext.GetAccount();
        return Redirect(account == null ? "/user/login" : $"/account/{account.Id}");
    }
    
    [Route("logout")]
    public async Task<IActionResult> Logout() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
    
    [Route("devicecode")]
    public async Task<IActionResult> DeviceCode()
        => Content(await OAuth2.DeviceCode(), "application/json");
    
    [Route("poll/{code}")]
    public async Task<IActionResult> Poll(string code) {
        var account = await HttpContext.GetAccount();
        if (account == null) return Unauthorized(new StatusModel {
            Message = "You must be logged in"
        });
        
        TokenPair? token;
        try {
            token = await OAuth2.PollToken(code);
        } catch {
            return BadRequest(new StatusModel {
                Message = "Device code has expired"
            });
        }
        
        if (token == null) return Ok(new StatusModel {
            Message = "Polling is still in progress"
        });
        
        try {
            var profile = new Profile {
                Instance = new MineProtocol.Authentication.Profile(token),
                Valid = true
            };
            
            await Database.Database.Profiles.InsertOneAsync(profile);
            return Ok(new StatusModel {
                Message = "Successfully added account", Success = true
            });
        } catch {
            return NotFound(new StatusModel {
                Message = "Account does not own Minecraft"
            });
        }
    }
}