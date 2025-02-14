using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ServerOverflow.Shared;

/// <summary>
/// Various extensions for convenience
/// </summary>
public static class Extensions {
    /// <summary>
    /// Queries the first item by an expression query
    /// </summary>
    /// <param name="collection">MongoDB collection</param>
    /// <param name="query">Expression query</param>
    /// <typeparam name="T">Collection type</typeparam>
    /// <returns>First element (can be null)</returns>
    public static async Task<T?> QueryFirst<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> query) {
        var filter = new ExpressionFilterDefinition<T>(query);
        using var res = await collection.FindAsync(filter, new FindOptions<T> { BatchSize = 1 });
        var list = res.ToList(); return list.Count == 0 ? default : list[0];
    }
    
    /// <summary>
    /// Queries the first item by an expression query
    /// </summary>
    /// <param name="collection">MongoDB collection</param>
    /// <param name="query">Expression query</param>
    /// <typeparam name="T">Collection type</typeparam>
    /// <returns>First element (can be null)</returns>
    public static async Task<List<T>> QueryAll<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> query) {
        var filter = new ExpressionFilterDefinition<T>(query);
        using var res = await collection.FindAsync(filter);
        return res.ToList();
    }
    
    /// <summary>
    /// Returns an estimated count of all elements in a collection
    /// </summary>
    /// <param name="collection">MongoDB collection</param>
    /// <typeparam name="T">Collection type</typeparam>
    /// <returns>Number of documents</returns>
    public static async Task<long> EstimatedCount<T>(this IMongoCollection<T> collection)
        => await collection.EstimatedDocumentCountAsync();
    
    /// <summary>
    /// Counts the amount of documents that fit specified query
    /// </summary>
    /// <param name="collection">MongoDB collection</param>
    /// <param name="query">Expression query</param>
    /// <typeparam name="T">Collection type</typeparam>
    /// <returns>Number of documents</returns>
    public static async Task<long> Count<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> query) {
        var filter = new ExpressionFilterDefinition<T>(query);
        return await collection.CountDocumentsAsync(filter);
    }
    
    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="collection">MongoDB collection</param>
    /// <param name="query">Expression query</param>
    /// <typeparam name="T">Collection type</typeparam>
    public static async Task Delete<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> query) {
        var filter = new ExpressionFilterDefinition<T>(query);
        await collection.DeleteOneAsync(filter);
    }
    
    /// <summary>
    /// Updates or inserts an item
    /// </summary>
    /// <param name="collection">MongoDB collection</param>
    /// <param name="query">Expression query</param>
    /// <param name="document">Update Document</param>
    /// <typeparam name="T">Collection type</typeparam>
    /// <returns>True if inserted, false otherwise</returns>
    public static async Task Upsert<T>(this IMongoCollection<T> collection, 
        Expression<Func<T, bool>> query, BsonDocument document) {
        var filter = new ExpressionFilterDefinition<T>(query);
        var update = new BsonDocumentUpdateDefinition<T>(document);
        await collection.UpdateOneAsync(filter, update, 
            new UpdateOptions { IsUpsert = true });
    }
    
    /// <summary>
    /// Generates a random string of length
    /// </summary>
    /// <param name="length">Length</param>
    /// <returns>Random string</returns>
    public static string RandomString(int length)
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(length / 2)).ToLower();
    
    /// <summary>
    /// Generates SHA-512 hash for specified string in hexadecimal
    /// </summary>
    /// <param name="str">String</param>
    /// <returns>SHA-512 hash</returns>
    public static string GetHash(this string str)
        => Encoding.UTF8.GetBytes(str).GetHash();
    
    /// <summary>
    /// Generates SHA-512 hash for specified byte buffer in hexadecimal
    /// </summary>
    /// <param name="buf">Byte buffer</param>
    /// <returns>SHA-512 hash</returns>
    public static string GetHash(this byte[] buf) 
        => Convert.ToHexString(SHA512.HashData(buf));

    /// <summary>
    /// Humanizes a pascal case string
    /// </summary>
    /// <param name="str">String</param>
    /// <returns>Humanized string</returns>
    public static string Humanize(this string str) {
        var words = new List<string>();
        var word = "";
        foreach (var ch in str) {
            if (char.IsUpper(ch) && word.Length > 0) {
                words.Add(word); word = "";
            }

            word += char.ToLower(ch);
        }
        
        words.Add(word);
        var res = string.Join(" ", words);
        return res[0].ToString().ToUpper() + res[1..];
    }
}