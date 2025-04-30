namespace ServerOverflow.Backend.Models;

/// <summary>
/// User registration model
/// </summary>
public class RegisterModel {
    /// <summary>
    /// Invitation code
    /// </summary>
    public string InviteCode { get; set; } = "";
    
    /// <summary>
    /// Desired username
    /// </summary>
    public string Username { get; set; } = "";
    
    /// <summary>
    /// Desired password
    /// </summary>
    public string Password { get; set; } = "";
}