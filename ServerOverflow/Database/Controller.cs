using static ServerOverflow.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace ServerOverflow.Database;

/// <summary>
/// Database controller
/// </summary>
public static class Controller {
    public static readonly IMongoCollection<Account> Accounts;
    public static readonly IMongoCollection<Invitation> Invitations;
    public static readonly IMongoCollection<Exclusion> Exclusions;
    public static readonly IMongoCollection<Profile> Profiles;
    public static readonly IMongoCollection<Server> Servers;
    public static readonly IMongoCollection<Player> Players;
    public static readonly IMongoCollection<BadIP> BadIPs;
    
    /// <summary>
    /// Initializes the MongoDB database
    /// </summary>
    static Controller() {
        Log.Information("Initializing MongoDB controller");
        var pack = new ConventionPack {
            new IgnoreExtraElementsConvention(true),
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register("ServerOverflow", pack, _ => true);
        var client = new MongoClient(Config.MongoUri);
        var database = client.GetDatabase("server-overflow");
        Invitations = database.GetCollection<Invitation>("invitations");
        Accounts = database.GetCollection<Account>("accounts");
        Profiles = database.GetCollection<Profile>("profiles");
        Players = database.GetCollection<Player>("players");
        Exclusions = database.GetCollection<Exclusion>("exclusions");
        Servers = database.GetCollection<Server>("servers");
        BadIPs = database.GetCollection<BadIP>("bad_servers");
    }
}