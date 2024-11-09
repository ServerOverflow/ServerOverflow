namespace MineProtocol.Schema;

/// <summary>
/// XSTS authentication request
/// </summary>
public class XstsAuthRequest {
    /// <summary>
    /// Properties class
    /// </summary>
    public class PropertiesClass {
        /// <summary>
        /// Sandbox identifier
        /// </summary>
        public string SandboxId { get; set; }= "RETAIL";
        
        /// <summary>
        /// List of user tokens
        /// </summary>
        public List<string> UserTokens { get; set; } = [];
    }

    /// <summary>
    /// Authentication properties
    /// </summary>
    public PropertiesClass Properties { get; set; } = new();
    
    /// <summary>
    /// Relying party URL
    /// </summary>
    public string RelyingParty = "rp://api.minecraftservices.com/";
    
    /// <summary>
    /// Type of token requested
    /// </summary>
    public string TokenType = "JWT";

    /// <summary>
    /// Creates a new XSTS live authentication request
    /// </summary>
    /// <param name="token">XBOX Token</param>
    public XstsAuthRequest(string token)
        => Properties.UserTokens.Add(token);
}