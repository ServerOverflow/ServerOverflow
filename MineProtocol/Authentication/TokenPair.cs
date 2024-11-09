namespace MineProtocol.Authentication; 

/// <summary>
/// Generic OAuth2 token pair
/// </summary>
public class TokenPair {
    /// <summary>
    /// Token for refreshing the access token
    /// </summary>
    public GenericToken? RefreshToken { get; set; }
        
    /// <summary>
    /// The access token itself
    /// </summary>
    public GenericToken? AccessToken { get; set; }
}