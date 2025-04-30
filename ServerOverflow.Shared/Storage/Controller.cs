using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace ServerOverflow.Shared.Storage;

/// <summary>
/// MongoDB database controller
/// </summary>
public static class Database {
    private static IMongoCollection<Invitation>? _invitations;
    private static IMongoCollection<Exclusion>? _exclusions;
    private static IMongoCollection<LogEntry>? _auditLogs;
    private static IMongoCollection<Account>? _accounts;
    private static IMongoCollection<Profile>? _profiles;
    private static IMongoCollection<ApiKey>? _apiKeys;
    private static IMongoCollection<Server>? _servers;
    private static IMongoCollection<Player>? _players;
    private static IMongoCollection<BadIP>? _badIPs;
    
    /// <summary>
    /// API keys database collection
    /// </summary>
    public static IMongoCollection<LogEntry> AuditLog {
        get {
            if (_auditLogs == null)
                throw new InvalidOperationException("Call Initialize first");
            return _auditLogs;
        }
    }
    
    /// <summary>
    /// API keys database collection
    /// </summary>
    public static IMongoCollection<ApiKey> ApiKeys {
        get {
            if (_apiKeys == null)
                throw new InvalidOperationException("Call Initialize first");
            return _apiKeys;
        }
    }
    
    /// <summary>
    /// Accounts database collection
    /// </summary>
    public static IMongoCollection<Account> Accounts {
        get {
            if (_accounts == null)
                throw new InvalidOperationException("Call Initialize first");
            return _accounts;
        }
    }
    
    /// <summary>
    /// Invitations database collection
    /// </summary>
    public static IMongoCollection<Invitation> Invitations {
        get {
            if (_invitations == null)
                throw new InvalidOperationException("Call Initialize first");
            return _invitations;
        }
    }
    
    /// <summary>
    /// Exclusions database collection
    /// </summary>
    public static IMongoCollection<Exclusion> Exclusions {
        get {
            if (_exclusions == null)
                throw new InvalidOperationException("Call Initialize first");
            return _exclusions;
        }
    }
    
    /// <summary>
    /// Profiles database collection
    /// </summary>
    public static IMongoCollection<Profile> Profiles {
        get {
            if (_profiles == null)
                throw new InvalidOperationException("Call Initialize first");
            return _profiles;
        }
    }
    
    /// <summary>
    /// Servers database collection
    /// </summary>
    public static IMongoCollection<Server> Servers {
        get {
            if (_servers == null)
                throw new InvalidOperationException("Call Initialize first");
            return _servers;
        }
    }
    
    /// <summary>
    /// Players database collection
    /// </summary>
    public static IMongoCollection<Player> Players {
        get {
            if (_players == null)
                throw new InvalidOperationException("Call Initialize first");
            return _players;
        }
    }
    
    /// <summary>
    /// Bad IPs database collection
    /// </summary>
    public static IMongoCollection<BadIP> BadIPs {
        get {
            if (_badIPs == null)
                throw new InvalidOperationException("Call Initialize first");
            return _badIPs;
        }
    }
    
    /// <summary>
    /// Initializes the MongoDB database
    /// </summary>
    /// <param name="uri">Connection URI</param>
    public static void Initialize(string uri) {
        ConventionRegistry.Register("ServerOverflow", 
            new ConventionPack { 
                new IgnoreExtraElementsConvention(true), 
                new CamelCaseElementNameConvention() 
            }, _ => true);
        
        Log.Information("Connecting to MongoDB database");
        var client = new MongoClient(uri);
        var database = client.GetDatabase("server-overflow");
        _invitations = database.GetCollection<Invitation>("invitations");
        _exclusions = database.GetCollection<Exclusion>("exclusions");
        _auditLogs = database.GetCollection<LogEntry>("audit_logs");
        _accounts = database.GetCollection<Account>("accounts");
        _profiles = database.GetCollection<Profile>("profiles");
        _apiKeys = database.GetCollection<ApiKey>("api_keys");
        _players = database.GetCollection<Player>("players");
        _servers = database.GetCollection<Server>("servers");
        _badIPs = database.GetCollection<BadIP>("bad_servers");
    }
}