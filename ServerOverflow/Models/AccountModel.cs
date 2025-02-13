using ServerOverflow.Storage;

namespace ServerOverflow.Models;

/// <summary>
/// Account model
/// </summary>
public class AccountModel : StatusModel {
    /// <summary>
    /// Target account for profile
    /// </summary>
    public Account Target { get; set; }
    
    /// <summary>
    /// Account of the user
    /// </summary>
    public Account Account { get; set; }
    
    /// <summary>
    /// Is target a different user
    /// </summary>
    public bool OtherTarget { get; set; }
}