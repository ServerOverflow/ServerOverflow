namespace MineProtocol.Schema;

/// <summary>
/// XBOX live authentication request
/// </summary>
public class XboxAuthRequest {
    /// <summary>
    /// Properties class
    /// </summary>
    public class PropertiesClass {
        /// <summary>
        /// Authentication method
        /// </summary>
        public string AuthMethod { get; set; } = "RPS";
        
        /// <summary>
        /// Authentication site name
        /// </summary>
        public string SiteName { get; set; } = "user.auth.xboxlive.com";
        
        /// <summary>
        /// RPS ticket + token
        /// </summary>
        public string RpsTicket { get; set; } = "d=";
    }

    /// <summary>
    /// Authentication properties
    /// </summary>
    public PropertiesClass Properties { get; set; } = new();
    
    /// <summary>
    /// Relying party URL
    /// </summary>
    public string RelyingParty { get; set; } = "http://auth.xboxlive.com";
    
    /// <summary>
    /// Type of token requested
    /// </summary>
    public string TokenType { get; set; } = "JWT";

    /// <summary>
    /// Creates a new XBOX live authentication request
    /// </summary>
    /// <param name="token">Access Token</param>
    public XboxAuthRequest(string token)
        => Properties.RpsTicket += token;
}