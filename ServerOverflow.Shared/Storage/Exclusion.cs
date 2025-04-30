using System.Net;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ServerOverflow.Shared.Converters;

namespace ServerOverflow.Shared.Storage;

/// <summary>
/// A scanner exclusion
/// </summary>
public class Exclusion {
    /// <summary>
    /// Document's object identifier
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }
    
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
        => await Database.Exclusions.QueryFirst(x => x.Id.ToString() == id);
    
    /// <summary>
    /// Fetches all exclusions
    /// </summary>
    /// <returns>List of exclusions</returns>
    public static async Task<List<Exclusion>> GetAll()
        => await Database.Exclusions.QueryAll(x => true);
    
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
        
        await Database.Exclusions.InsertOneAsync(exclusion);
        return exclusion;
    }
    
    /// <summary>
    /// Updates whole document
    /// </summary>
    public async Task Update() {
        var filter = Builders<Exclusion>.Filter.Eq(account => account.Id, Id);
        await Database.Exclusions.ReplaceOneAsync(filter, this);
    }

    /// <summary>
    /// Checks if an IP matches an exclusion
    /// </summary>
    /// <param name="ip">IP address</param>
    /// <returns>True if matches</returns>
    public bool IsExcluded(string ip)
        => Ranges.Any(range => {
            if (!range.Contains('/')) 
                return ip == range;
            var parts = range.Split('/');
            return InRange(
                IPAddress.Parse(ip),
                IPAddress.Parse(parts[0]),
                int.Parse(parts[1]));
        });
    
    /// <summary>
    /// Checks if an IP is in the IP range
    /// </summary>
    /// <param name="ip">IP address</param>
    /// <param name="range">IP range</param>
    /// <param name="subnet">Subnet</param>
    /// <returns>True if is in range</returns>
    private static bool InRange(IPAddress ip, IPAddress range, int subnet) {
        var addressBytes = ip.GetAddressBytes();
        var baseBytes = range.GetAddressBytes();
        var byteCount = subnet / 8;
        var bitCount = subnet % 8;

        for (var i = 0; i < byteCount; i++)
            if (addressBytes[i] != baseBytes[i])
                return false;

        if (bitCount <= 0) return true;
        var mask = (byte)~(255 >> bitCount);
        return (addressBytes[byteCount] & mask) == (baseBytes[byteCount] & mask);
    }
    
    /// <summary>
    /// Counts the total number of IP addresses
    /// </summary>
    /// <returns>Total number of IPs</returns>
    public long Count() {
        long total = 0;

        foreach (var range in Ranges) {
            if (!range.Contains('/')) {
                total += 1;
            } else {
                var parts = range.Split('/');
                var subnet = int.Parse(parts[1]);
                if (IPAddress.Parse(parts[0]).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    total += 1L << (32 - subnet);
            }
        }

        return total;
    }
}