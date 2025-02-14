using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServerOverflow.Frontend.Models;
using ServerOverflow.Frontend.Processors;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ServerOverflow.Frontend.Controllers;

/// <summary>
/// Information controller
/// </summary>
public class InfoController : Controller {
    [Route("server/{id}")]
    public async Task<IActionResult> Server(string id) {
        var account = await HttpContext.GetAccount();
        var server = await Shared.Storage.Server.Get(id);
        if (server == null) return NotFound();
        ViewData["Title"] = $"{server.IP}:{server.Port}";
        ViewData["Image"] = server.Ping.Favicon != null 
            ? $"/favicon/{server.Id}"
            : "/img/default.png";
        ViewData["Description"] = 
            server.Ping.DescriptionToText()?.Split("\n")[0] 
            ?? "Click to view server";
        
        if (account == null)
            return View("Error", new ErrorModel {
                StatusCode = StatusCodes.Status401Unauthorized
            });
        
        if (!account.HasPermission(Permission.SearchServers))
            return View("Error", new ErrorModel {
                StatusCode = StatusCodes.Status403Forbidden
            });

        var model = new GenericModel<Server> {
            Item = server
        };
        
        if (!HttpContext.Request.HasFormContentType
            || !HttpContext.Request.Form.TryGetValue(
                "type", out var type))
            return View(model);

        switch (type) {
            case "refresh": { // Refresh
                var profiles = await Profile.GetAll();
                Profile? profile = null;
                if (profiles.Count != 0)
                    profile = profiles[RandomNumberGenerator.GetInt32(profiles.Count)];
                var result = await MinecraftBot.Join(server, profile?.Instance);
                await Database.Servers.UpdateOneAsync(
                    Builders<Server>.Filter.Eq(y => y.Id, server.Id),
                    Builders<Server>.Update.Set(x => x.JoinResult, result));
                server.JoinResult = result;
                break;
            }
        }
        
        return View(model);
    }
    
    [Route("account/{id}")]
    public async Task<IActionResult> Account(string id) {
        var account = await HttpContext.GetAccount();
        var target = await Shared.Storage.Account.Get(id);
        if (target == null) return NotFound();
        ViewData["Image"] = "/img/user.png";
        ViewData["Title"] = target.Username;
        ViewData["Description"] = "Click to view account";
        
        if (account == null)
            return View("Error", new ErrorModel {
                StatusCode = StatusCodes.Status401Unauthorized
            });

        var model = new AccountModel {
            OtherTarget = account.Id != target.Id,
            Account = account, Target = target
        };
        
        if (!model.Account.HasPermission(Permission.SearchAccounts) && model.OtherTarget)
            return View("Error", new ErrorModel {
                StatusCode = StatusCodes.Status403Forbidden
            });
        
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

                await Database.Accounts.Delete(x => x.Id == model.Target.Id);
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
                
                if (await Shared.Storage.Account.GetByName(username!) != null) {
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