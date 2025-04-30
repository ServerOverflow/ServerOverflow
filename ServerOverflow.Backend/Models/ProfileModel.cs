using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Models;

/// <summary>
/// Minecraft profile model
/// </summary>
public class ProfileModel {
    /// <summary>
    /// Username of the account
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// UUID of the account
    /// </summary>
    public string? UUID { get; set; }
    
    /// <summary>
    /// Is the account still valid
    /// </summary>
    public bool Valid { get; set; }
    
    /// <summary>
    /// Empty constructor for JSON
    /// </summary>
    public ProfileModel() {}

    /// <summary>
    /// Creates a new minecraft profile model
    /// </summary>
    /// <param name="profile">Profile</param>
    public ProfileModel(Profile profile) {
        Username = profile.Instance.Username;
        UUID = profile.Instance.UUID;
        Valid = profile.Valid;
    }
}