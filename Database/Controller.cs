using MongoDB.Driver;
using Serilog;

namespace ServerOverflow.Database;

/// <summary>
/// Database controller
/// </summary>
public static class Controller {
    //public static readonly IMongoCollection<User> Users;
    //public static readonly IMongoCollection<Server> Servers;
    
    /// <summary>
    /// Initializes the MongoDB database
    /// </summary>
    static Controller() {
        Log.Information("Initializing MongoDB controller");
        var client = new MongoClient(new MongoClientSettings {
            Server = new MongoServerAddress("localhost"),
            MaxConnectionPoolSize = 500
        });
        var database = client.GetDatabase("server-overflow");
        //Users = database.GetCollection<User>("users");
        var matscan = client.GetDatabase("matscan");
        //Servers = database.GetCollection<Server>("servers");
    }
}