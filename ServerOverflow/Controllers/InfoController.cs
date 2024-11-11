using Microsoft.AspNetCore.Mvc;
using ServerOverflow.Database;
using ServerOverflow.Models;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ServerOverflow.Controllers;

/// <summary>
/// Information controller
/// </summary>
public class InfoController : Controller {
    [Route("server/{id}")]
    public async Task<IActionResult> Server(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");
        var server = await Database.Server.Get(id);
        if (server == null) return NotFound();
        if (!account.HasPermission(Permission.SearchServers))
            return Unauthorized();

        return View(new GenericModel<Server> {
            Item = server
        });
    }
    
    [Route("account/{id?}")]
    public async Task<IActionResult> Account(string? id) {
        var account = await HttpContext.GetAccount();
        if (account == null) return Redirect("/user/login");
        var model = new AccountModel {
            Account = account,
            Target = account
        };

        if (id != null) {
            var target = await Database.Account.Get(id);
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

                if (model.Target.Invitee == null) {
                    model.Message = "You can't modify permissions of the owner!";
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
                
                if (model.Target.Invitee == null && model.OtherTarget) {
                    model.Message = "Only the owner can change his own password!";
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
                    model.Message = "Only administrators can generate API token for other accounts!";
                    break;
                }
                
                if (model.Target.Invitee == null && model.OtherTarget) {
                    model.Message = "Only the owner can generate an API token for himself!";
                    break;
                }

                model.Target.ApiKey = Extensions.RandomString(32);
                await model.Target.Update();
                model.Message = $"Here is your new API token: {model.Target.ApiKey}";
                model.Success = true;
                break;
            case "deleteAccount": // Delete Account
                if (model.OtherTarget && !account.HasPermission(Permission.Administrator)) {
                    model.Message = "Only administrators can delete other accounts!";
                    break;
                }
                
                if (model.Target.Invitee == null) {
                    model.Message = "You can't delete the owner's account!";
                    break;
                }

                await Database.Controller.Accounts.Delete(x => x.Id == model.Target.Id);
                return Redirect("/");
            case "changeUsername": { // Change Username
                if (model.OtherTarget && !account.HasPermission(Permission.ModifyAccounts)) {
                    model.Message = "Modify accounts permission is required to change someone else's username!";
                    break;
                }
                
                if (model.Target.Invitee == null && model.OtherTarget) {
                    model.Message = "Only the owner can change his own username!";
                    break;
                }
                    
                if (!HttpContext.Request.Form.TryGetValue(
                        "username", out var username))
                    break;
                
                if (await Database.Account.GetByName(username!) != null) {
                    model.Message = "This username has already been taken!";
                    break;
                }
            
                model.Target.Username = username!;
                await model.Target.Update();
                model.Message = "Successfully changed your account's username!";
                model.Success = true;
                break;
            }
        }
        
        return View(model);
    }
}