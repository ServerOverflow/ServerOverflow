namespace ServerOverflow.Database;

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