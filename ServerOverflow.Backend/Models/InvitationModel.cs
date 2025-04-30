using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Models;

/// <summary>
/// Invitation model
/// </summary>
public class InvitationModel {
    /// <summary>
    /// Invitation's identifier
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Invitation code
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// New account's badge
    /// </summary>
    public string? BadgeText { get; set; } = "";
    
    /// <summary>
    /// User ID of who generated the invitation code
    /// </summary>
    public string? Creator { get; set; }
    
    /// <summary>
    /// Was the invitation code used
    /// </summary>
    public bool? Used { get; set; }
    
    /// <summary>
    /// Empty constructor for JSON
    /// </summary>
    public InvitationModel() {}

    /// <summary>
    /// Creates a new user model
    /// </summary>
    /// <param name="invite">Invitation</param>
    public InvitationModel(Invitation invite) {
        Id = invite.Id.ToString();
        Code = invite.Code;
        BadgeText = invite.BadgeText;
        Creator = invite.Creator.ToString();
        Used = invite.Used;
    }
}