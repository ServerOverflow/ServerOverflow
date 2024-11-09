using System.Data;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using ServerOverflow.Database;

namespace ServerOverflow;

/// <summary>
/// ServerOverflow's advanced query processor
/// </summary>
public static class Query {
    /// <summary>
    /// Generates a filter from a server query.
    /// Throws an exception in case of a syntax error.
    /// </summary>
    /// <returns>BSON filter</returns>
    public static BsonDocument Servers(string query)
        => Generate(query, (op, reversed, content, filter) => {
            switch (op) {
                case "botJoined": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    filter.Add("joinResult", content == "true"
                        ? new BsonDocument("$and", new BsonDocument{
                            new BsonElement("$exists", "true"),
                            new BsonElement("$ne", null)
                        })
                        : new BsonDocument("$or", new BsonDocument{
                            new BsonElement("$exists", "false"),
                            new BsonElement("$eq", null)
                        }));
                    break;
                }
                case "allowsReporting": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    filter.Add("minecraft.enforcesSecureChat", new BsonDocument("$eq", content == "true"));
                    break;
                }
                case "hasForge": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    filter.Add("minecraft.isForge", new BsonDocument("$eq", content == "true"));
                    break;
                }
                case "onlineMode": {
                    filter.Add("onlineModeGuess", new BsonDocument("$eq", content switch {
                        "offline" => OnlineMode.Offline,
                        "online" => OnlineMode.Online,
                        _ => OnlineMode.Mixed
                    }));
                    break;
                }
                case "online": {
                    if (!uint.TryParse(content, out var number))
                        throw new SyntaxErrorException($"Expected an unsigned number, found {content} instead");
                    filter.Add("minecraft.players.online", new BsonDocument(reversed ? "$ne" : "$eq", number));
                    break;
                }
                case "max": {
                    if (!uint.TryParse(content, out var number))
                        throw new SyntaxErrorException($"Expected an unsigned number, found {content} instead");
                    filter.Add("minecraft.players.max", new BsonDocument(reversed ? "$ne" : "$eq", number));
                    break;
                }
                case "protocol": {
                    if (!uint.TryParse(content, out var number))
                        throw new SyntaxErrorException($"Expected an unsigned number, found {content} instead");
                    filter.Add("minecraft.version.protocol", new BsonDocument(reversed ? "$ne" : "$eq", number));
                    break;
                }
                case "version": {
                    var regex = new BsonRegularExpression(content, "i");
                    filter.Add("minecraft.version.name", new BsonDocument(
                        reversed ? "$not" : "$regex", regex));
                    break;
                }
                case "hasPlayer": {
                    if (Guid.TryParse(content, out _)) {
                        filter.Add("minecraft.players.sample.id", new BsonDocument(
                            reversed ? "$ne" : "$eq", content));
                        break;
                    }
                    
                    filter.Add("minecraft.players.sample.name", new BsonDocument(
                        reversed ? "$ne" : "$eq", content));
                    break;
                }
                case "hasMod": {
                    filter.Add("$or", new BsonArray {
                        new BsonDocument("minecraft.forgeData.modId", 
                            new BsonDocument(reversed ? "$ne" : "$eq", content)),
                        new BsonDocument("minecraft.modinfo.modid", 
                            new BsonDocument(reversed ? "$ne" : "$eq", content))
                    });
                    break;
                }
                default: throw new SyntaxErrorException(
                    $"Invalid operator \"{op}\"");
            }
        }, "minecraft.cleanDescription");

    /// <summary>
    /// The shared portion of advanced query language
    /// </summary>
    /// <param name="query">Query String</param>
    /// <param name="handler">Operator Handler</param>
    /// <param name="def">Default Field</param>
    /// <returns>BSON filter</returns>
    private static BsonDocument Generate(string query, 
        Action<string, bool, string, BsonDocument> handler, string def) {
        var buffer = ""; var multi = false;
        var filter = new BsonDocument();
        void HandleOperator() {
            var index = buffer.IndexOf(':');
            var content = buffer[(index + 1)..];
            if (content.StartsWith('\"'))
                content = content[1..^1];
            var op = buffer[..index];
            var reversed = false;
            if (op.StartsWith('-')) {
                reversed = true;
                op = op[1..];
            }
            handler(op, reversed, content, filter);
            buffer = null; multi = false;
        }

        var clean = "";
        foreach (var i in query.Split(" ")) {
            if (string.IsNullOrWhiteSpace(i)) continue;
            var isEnd = i.EndsWith('\"') && (i.Length == 1 || i[^1] != '\\');
            if (multi && isEnd) {
                if (!i.EndsWith('\"')) throw new SyntaxErrorException(
                    "The ending quote must always be the last character in a word");
                buffer += i;
                if (!buffer.StartsWith('\"')) HandleOperator();
                else clean += $"|{Regex.Escape(buffer[1..^1])}";
                buffer = null; multi = false;
                continue;
            }

            var last = '\0';
            var isOperator = false;
            foreach (var ch in i) {
                if (last != '\\' && ch == '\"') {
                    if (!multi) {
                        if (last != '\0' && !isOperator) throw new SyntaxErrorException(
                            "A starting quote is only allowed at the start of an word, or after the operator's colon");
                        multi = true;
                    } else multi = false;
                }

                if (!multi && last != '\\' && ch == ':')
                    isOperator = true;
                buffer += ch; last = ch;
            }
            
            if (buffer!.StartsWith('\"') && !multi) {
                buffer = buffer[1..^1];
                isOperator = false;
            }
            
            if (multi) buffer += " ";
            else if (isOperator) HandleOperator();
            else clean += $"|{Regex.Escape(buffer)}";
        }
        
        if (multi) throw new SyntaxErrorException(
            "Multi-word quotation wasn't closed");
        if (!string.IsNullOrWhiteSpace(clean))
            filter.Add(def, new BsonDocument {
                new BsonElement("$regex", clean[1..]),
                new BsonElement("$options", "i")
            });
        return filter;
    }
}