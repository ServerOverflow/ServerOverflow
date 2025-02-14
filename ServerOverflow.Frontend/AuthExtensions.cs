using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Frontend;

/// <summary>
/// Various extensions for convenience
/// </summary>
public static class AuthExtensions {
    /// <summary>
    /// Signs in using specified account
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="account">Account</param>
    public static async Task SignIn(this HttpContext context, Account account)
        => await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim> {
                    new(ClaimTypes.Name, account.Id.ToString()!),
                }, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties {
                RedirectUri = "/user/profile",
                IsPersistent = true
            });

    /// <summary>
    /// Get an account from current identity
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User's account</returns>
    public static async Task<Account?> GetAccount(this HttpContext context) {
        var name = context.User.Identity?.Name;
        if (name == null) return null;
        return await Account.Get(name);
    }
}