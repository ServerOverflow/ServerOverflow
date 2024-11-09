using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Database;
using ServerOverflow.Models;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

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
            model.Message = "Invalid invitation code!";
            return View(model);
        }
        
        if (invite.Used) {
            model.Message = "This invitation code has already been used!";
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
            model.Message = "Wrong username or password!";
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

                await Database.Controller.Invitations.Delete(x => x.Id.ToString() == id.ToString());
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

                await Database.Controller.Exclusions.Delete(x => x.Id.ToString() == id.ToString());
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
            case "deleteAccount": { // Delete account (stub)
                break;
            }
        }
        
        return View(model);
    }

    [Route("profile/{id?}")]
    public async Task<IActionResult> Profile(string? id) {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");
        var model = new AccountModel {
            Account = account,
            Target = account
        };

        if (id != null) {
            var target = await Account.Get(id);
            if (target == null) return NotFound();
            if (!account.HasPermission(Permission.SearchAccounts) && account.Id != target.Id)
                return Unauthorized();
            model.OtherTarget = true;
            model.Target = target;
        }
        
        if (!HttpContext.Request.HasFormContentType
            || !HttpContext.Request.Form.TryGetValue(
                "type", out var type))
            return View(model);
        
        switch (type.ToString()) {
            case "changePermissions": // Change Permissions
                if (!account.HasPermission(Permission.Administrator)) {
                    model.Message = "Only administrators can modify permissions!";
                    break;
                }
                    
                model.Target.Permissions.Clear();
                foreach (var pair in HttpContext.Request.Form) {
                    if (!pair.Key.StartsWith("perm-")) continue;
                    var perm = (Permission) int.Parse(pair.Key[5..]);
                    if (pair.Value[0] == "on") model.Target.Permissions.Add(perm);
                }
                    
                await model.Target.Update();
                model.Message = "Successfully modified this account's permissions!";
                model.Success = true;
                break;
            case "changePassword": // Change Password
                if (model.OtherTarget && !account.HasPermission(Permission.ModifyAccounts)) {
                    model.Message = "Modify accounts permission is required to change someone else's password!";
                    break;
                }
                    
                if (!HttpContext.Request.Form.TryGetValue(
                        "password", out var oldPassword))
                    break;
                if (!HttpContext.Request.Form.TryGetValue(
                        "password2", out var newPassword))
                    break;
                if (!account.HasPermission(Permission.Administrator) 
                    && model.Target.CheckPassword(oldPassword!)) {
                    model.Message = "Wrong old password specified!";
                    break;
                }

                await model.Target.ChangePassword(newPassword!);
                model.Message = "Successfully changed your account's password!";
                model.Success = true;
                break;
            case "generateApiKey": // Generate API key
                if (model.OtherTarget && !account.HasPermission(Permission.Administrator)) {
                    model.Message = "Only administrators can generate API keys for other accounts!";
                    break;
                }

                model.Target.ApiKey = Extensions.RandomString(16);
                await model.Target.Update();
                model.Message = $"Here is your new API key: {model.Target.ApiKey}";
                model.Success = true;
                break;
            case "deleteAccount": // Delete Account
                if (model.OtherTarget && !account.HasPermission(Permission.Administrator)) {
                    model.Message = "Only administrators can delete other accounts!";
                    break;
                }

                await Database.Controller.Accounts.Delete(x => x.Id == model.Target.Id);
                return Redirect("/");
            case "changeUsername": { // Change Username
                if (model.OtherTarget && !account.HasPermission(Permission.ModifyAccounts)) {
                    model.Message = "Modify accounts permission is required to change someone else's username!";
                    break;
                }
                    
                if (!HttpContext.Request.Form.TryGetValue(
                        "username", out var username))
                    break;
            
                model.Target.Username = username!;
                await model.Target.Update();
                model.Message = "Successfully changed your account's username!";
                model.Success = true;
                break;
            }
        }
        
        return View(model);
    }
    
    [Route("logout")]
    public async Task<IActionResult> Logout() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}