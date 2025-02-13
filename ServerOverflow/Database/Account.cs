using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ServerOverflow.Database;

/// <summary>
/// A user's account
/// </summary>
public class Account {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
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
    public string ApiKey { get; set; } = Extensions.RandomString(32);
    
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
        => await Database.Accounts.QueryFirst(x => x.Id.ToString() == id);
    
    /// <summary>
    /// Fetches an account by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>Account, may be null</returns>
    public static async Task<Account?> GetByName(string username)
        => await Database.Accounts.QueryFirst(x => x.Username.ToLower() == username);
    
    /// <summary>
    /// Fetches an account by username and password
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <returns>Account, may be null</returns>
    public static async Task<Account?> Get(string username, string password)
        => await Database.Accounts.QueryFirst(x => x.Username.ToLower() == username.ToLower() && x.Password == password.GetHash());
    
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
        
        var accounts = await Database.Accounts.EstimatedCount();
        if (accounts == 0) account.Permissions.Add(Permission.Administrator);
        await Database.Accounts.InsertOneAsync(account);
        invitation.Used = true; await invitation.Update();
        return account;
    }
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Account>.Filter.Eq(account => account.Id, Id);
        await Database.Accounts.ReplaceOneAsync(filter, this);
    }
}

/// <summary>
/// A list of user permissions
/// </summary>
public enum Permission {
    /// <summary>
    /// Grants every single permission and disallows anyone else from modifying your profile
    /// </summary>
    Administrator = 0,
    
    /// <summary>
    /// Allow searching servers
    /// </summary>
    SearchServers = 1,
    
    /// <summary>
    /// Allow searching players
    /// </summary>
    SearchPlayers = 2,
    
    /// <summary>
    /// Allows searching accounts
    /// </summary>
    SearchAccounts = 3,
    
    /// <summary>
    /// Allows modification of other user accounts
    /// </summary>
    ModifyAccounts = 4,

    /// <summary>
    /// Allows modifying the scanner exclusion list
    /// </summary>
    ModifyExclusions = 5
}