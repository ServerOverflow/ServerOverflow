namespace MineProtocol.Authentication; 

/// <summary>
/// A generic token
/// </summary>
public class GenericToken {
    /// <summary>
    /// The token itself
    /// </summary>
    public string Token { get; set; }
    
    /// <summary>
    /// Date of token expiration
    /// </summary>
    public DateTime ExpireAt { get; set; }

    /// <summary>
    /// Creates a new generic token
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="expireAt">Expiration date</param>
    public GenericToken(string token, DateTime expireAt) {
        Token = token; ExpireAt = expireAt;
    }
}