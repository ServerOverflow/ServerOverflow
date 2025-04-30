using MongoDB.Bson;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Models;

public class UserModel {
    /// <summary>
    /// Users account identifier
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Account's username
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// User's badge displayed in their profile
    /// </summary>
    public string? BadgeText { get; set; }
    
    /// <summary>
    /// New password for users account
    /// </summary>
    public string? NewPassword { get; set; }
    
    /// <summary>
    /// The account ID of the invitee
    /// </summary>
    public string? Invitee { get; set; }

    /// <summary>
    /// A list of user permissions
    /// </summary>
    public List<Permission>? Permissions { get; set; }
    
    /// <summary>
    /// Empty constructor for JSON
    /// </summary>
    public UserModel() {}

    /// <summary>
    /// Creates a new user model
    /// </summary>
    /// <param name="account">Account</param>
    public UserModel(Account account) {
        Permissions = account.Permissions;
        BadgeText = account.BadgeText;
        Username = account.Username;
        Invitee = account.Invitee.ToString();
        Id = account.Id.ToString();
    }
}