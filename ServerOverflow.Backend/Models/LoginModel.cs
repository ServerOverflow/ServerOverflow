namespace ServerOverflow.Backend.Models;

/// <summary>
/// User login model
/// </summary>
public class LoginModel {
    /// <summary>
    /// Account's username
    /// </summary>
    public string Username { get; set; } = "";
    
    /// <summary>
    /// Account's password
    /// </summary>
    public string Password { get; set; } = "";
}