using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ServerOverflow.Storage; 

/// <summary>
/// An invitation code
/// </summary>
public class Invitation {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
    /// <summary>
    /// Invitation code
    /// </summary>
    public string Code { get; set; } = Extensions.RandomString(8);
    
    /// <summary>
    /// New account's badge
    /// </summary>
    public string BadgeText { get; set; } = "";
    
    /// <summary>
    /// User who generated the invitation code
    /// </summary>
    public ObjectId? Creator { get; set; }
    
    /// <summary>
    /// Was the invitation code used
    /// </summary>
    public bool Used { get; set; }

    /// <summary>
    /// Fetches an invitation by its code
    /// </summary>
    /// <param name="code">Invitation code</param>
    /// <returns>Invitation, may be null</returns>
    public static async Task<Invitation?> Get(string code)
        => await Database.Invitations.QueryFirst(x => x.Code == code);

    /// <summary>
    /// Fetches all invitations created
    /// </summary>
    /// <returns>List of invitations</returns>
    public static async Task<List<Invitation>> GetAll()
        => await Database.Invitations.QueryAll(x => true);

    /// <summary>
    /// Creates a new invitation code
    /// </summary>
    /// <param name="badge">Badge text</param>
    /// <param name="creator">Creator</param>
    /// <returns>Invitation</returns>
    public static async Task<Invitation> Create(string badge, Account? creator = null) {
        var invitation = new Invitation {
            Creator = creator?.Id,
            BadgeText = badge
        };

        await Database.Invitations.InsertOneAsync(invitation);
        return invitation;
    }
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Invitation>.Filter.Eq(account => account.Id, Id);
        await Database.Invitations.ReplaceOneAsync(filter, this);
    }
}