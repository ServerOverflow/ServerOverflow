using MongoDB.Driver;

namespace ServerOverflow.Database;

/// <summary>
/// A scanner exclusion
/// </summary>
public class Exclusion : Document {
    /// <summary>
    /// List of IP ranges to exclude
    /// </summary>
    public List<string> Ranges { get; set; } = [];

    /// <summary>
    /// Comment for this exclusion
    /// </summary>
    public string Comment { get; set; } = "";
    
    /// <summary>
    /// Fetches exclusion by its object id
    /// </summary>
    /// <param name="id">Exclusion ID</param>
    /// <returns>Exclusion, may be null</returns>
    public static async Task<Exclusion?> Get(string id)
        => await Controller.Exclusions.QueryFirst(x => x.Id.ToString() == id);
    
    /// <summary>
    /// Fetches all exclusions
    /// </summary>
    /// <returns>List of exclusions</returns>
    public static async Task<List<Exclusion>> GetAll()
        => await Controller.Exclusions.QueryAll(x => true);
    
    /// <summary>
    /// Creates a new exclusion
    /// </summary>
    /// <param name="ranges">IP ranges</param>
    /// <param name="comment">Comment</param>
    /// <returns>Exclusion</returns>
    public static async Task<Exclusion> Create(string[] ranges, string comment) {
        var exclusion = new Exclusion {
            Ranges = ranges.ToList(),
            Comment = comment
        };
        
        await Controller.Exclusions.InsertOneAsync(exclusion);
        return exclusion;
    }
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Exclusion>.Filter.Eq(account => account.Id, Id);
        await Controller.Exclusions.ReplaceOneAsync(filter, this);
    }
}