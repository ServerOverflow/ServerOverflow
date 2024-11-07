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
    public static readonly IMongoCollection<Server> Servers;
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
        var client = new MongoClient(new MongoClientSettings {
            Server = new MongoServerAddress("localhost"),
            MaxConnectionPoolSize = 500
        });
        var database = client.GetDatabase("server-overflow");
        Accounts = database.GetCollection<Account>("accounts");
        Invitations = database.GetCollection<Invitation>("invitations");
        var matscan = client.GetDatabase("matscan");
        Exclusions = matscan.GetCollection<Exclusion>("exclusions");
        Servers = matscan.GetCollection<Server>("servers");
        BadIPs = matscan.GetCollection<BadIP>("bad_servers");
    }
}