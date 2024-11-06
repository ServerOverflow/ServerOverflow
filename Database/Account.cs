using MongoDB.Bson;
using MongoDB.Driver;

namespace ServerOverflow.Database;

/// <summary>
/// A user's account
/// </summary>
public class Account : Document {
    /// <summary>
    /// Account's username
    /// </summary>
    public string Username { get; set; } = "";
    
    /// <summary>
    /// SHA-512 hash of the password
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// User's badge displayed in their profile
    /// </summary>
    public string BadgeText { get; set; } = "";
    
    /// <summary>
    /// The account ID of the invitee
    /// </summary>
    public ObjectId? Invitee { get; set; }

    /// <summary>
    /// A list of user permissions
    /// </summary>
    public List<Permission> Permissions { get; set; } = [];
    
    /// <summary>
    /// User's unique API key
    /// </summary>
    public string ApiKey { get; set; } = Extensions.RandomString(16);
    
    /// <summary>
    /// Checks if the account has a permission
    /// </summary>
    /// <returns>True if user has permission</returns>
    public bool HasPermission(Permission perm) 
        => Permissions.Contains(Permission.Administrator) || Permissions.Contains(perm);
    
    /// <summary>
    /// Checks if specified password is correct
    /// </summary>
    /// <param name="password">Password</param>
    /// <returns>True if correct</returns>
    public bool CheckPassword(string password)
        => Password.GetHash() == password;

    /// <summary>
    /// Changes account's password
    /// </summary>
    /// <param name="password">Password</param>
    public async Task ChangePassword(string password) {
        Password = password.GetHash(); await Update();
    }
    
    /// <summary>
    /// Fetches an account by its identifier
    /// </summary>
    /// <param name="id">Object identifier</param>
    /// <returns>Account, may be null</returns>
    public static async Task<Account?> Get(string id)
        => await Controller.Accounts.QueryFirst(x => x.Id.ToString() == id);
    
    /// <summary>
    /// Fetches an account by username and password
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <returns>Account, may be null</returns>
    public static async Task<Account?> Get(string username, string password)
        => await Controller.Accounts.QueryFirst(x => x.Username == username && x.Password == password.GetHash());
    
    /// <summary>
    /// Creates a new account and invalidates specified invitation
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <param name="invitation">Invitation</param>
    /// <returns>Account</returns>
    public static async Task<Account> Create(string username, string password, Invitation invitation) {
        var account = new Account {
            Username = username,
            Password = password.GetHash(),
            BadgeText = invitation.BadgeText,
            Invitee = invitation.Creator
        };
        
        var accounts = await Controller.Accounts.EstimatedCount();
        if (accounts == 0) {
            account.Permissions.Add(Permission.Administrator);
        }
        
        await Controller.Accounts.InsertOneAsync(account);
        invitation.Used = true; await invitation.Update();
        return account;
    }
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Account>.Filter.Eq(account => account.Id, Id);
        await Controller.Accounts.ReplaceOneAsync(filter, this);
    }
}