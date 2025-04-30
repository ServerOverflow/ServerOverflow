using System.Data;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend;

/// <summary>
/// ServerOverflow's advanced query processor
/// </summary>
public static class Query {
    /// <summary>
    /// Generates a filter from an exclusions query.
    /// Throws an exception in case of a syntax error.
    /// </summary>
    /// <returns>BSON filter</returns>
    public static FilterDefinition<Exclusion> Exclusions(string query)
        => Generate<Exclusion>(query, (op, reversed, content) => {
            switch (op) {
                case "ip": {
                    return reversed
                        ? Builders<Exclusion>.Filter.Not(Builders<Exclusion>.Filter.AnyEq(x => x.Ranges, content))
                        : Builders<Exclusion>.Filter.AnyEq(x => x.Ranges, content);
                }
                default: throw new SyntaxErrorException(
                    $"Invalid operator \"{op}\"");
            }
        }, x => x.Comment);
    
    /// <summary>
    /// Generates a filter from a server query.
    /// Throws an exception in case of a syntax error.
    /// </summary>
    /// <returns>BSON filter</returns>
    public static FilterDefinition<Server> Servers(string query)
        => Generate<Server>(query, (op, reversed, content) => {
            switch (op) {
                case "botJoined": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    return content == "true" 
                        ? Builders<Server>.Filter.Ne(x => x.JoinResult, null)
                        : Builders<Server>.Filter.Eq(x => x.JoinResult, null);
                }
                case "allowsReporting": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    return Builders<Server>.Filter.Eq(x => x.Ping.ChatReporting, content == "true");
                }
                case "hasForge": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    return Builders<Server>.Filter.Eq(x => x.Ping.IsForge, content == "true");
                }
                case "onlineMode": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    return Builders<Server>.Filter.Eq(x => x.JoinResult!.OnlineMode, content == "true");
                }
                case "whitelist": {
                    if (reversed) throw new SyntaxErrorException("Boolean operators do not allow reversing");
                    if (content is not "true" and not "false") throw new SyntaxErrorException(
                        $"Expected a binary true or false, found {content} instead");
                    return Builders<Server>.Filter.Eq(x => x.JoinResult!.Whitelist, content == "true");
                }
                case "online": {
                    if (!uint.TryParse(content, out var number))
                        throw new SyntaxErrorException($"Expected an unsigned number, found {content} instead");
                    return reversed
                        ? Builders<Server>.Filter.Ne(x => x.Ping.Players!.Online, (int)number)
                        : Builders<Server>.Filter.Eq(x => x.Ping.Players!.Online, (int)number);
                }
                case "max": {
                    if (!uint.TryParse(content, out var number))
                        throw new SyntaxErrorException($"Expected an unsigned number, found {content} instead");
                    return reversed
                        ? Builders<Server>.Filter.Ne(x => x.Ping.Players!.Max, (int)number)
                        : Builders<Server>.Filter.Eq(x => x.Ping.Players!.Max, (int)number);
                }
                case "protocol": {
                    if (!uint.TryParse(content, out var number))
                        throw new SyntaxErrorException($"Expected an unsigned number, found {content} instead");
                    return reversed
                        ? Builders<Server>.Filter.Ne(x => x.Ping.Version!.Protocol, (int)number)
                        : Builders<Server>.Filter.Eq(x => x.Ping.Version!.Protocol, (int)number);
                }
                case "ip": {
                    return reversed
                        ? Builders<Server>.Filter.Ne(x => x.IP, content)
                        : Builders<Server>.Filter.Eq(x => x.IP, content);
                }
                case "port": {
                    if (!ushort.TryParse(content, out var number))
                        throw new SyntaxErrorException($"Expected an unsigned short, found {content} instead");
                    return reversed
                        ? Builders<Server>.Filter.Ne(x => x.Port, number)
                        : Builders<Server>.Filter.Eq(x => x.Port, number);
                }
                case "version": {
                    if (reversed) throw new SyntaxErrorException("Version operator do not allow reversing");
                    var regex = new BsonRegularExpression(content, "i");
                    return Builders<Server>.Filter.Regex(x => x.Ping.Version!.Name, regex);
                }
                case "hasPlayer": {
                    if (reversed) throw new SyntaxErrorException("Has player operator do not allow reversing");
                    if (!Guid.TryParse(content, out _)) throw new SyntaxErrorException("Only UUIDs are supported");
                    return Builders<Server>.Filter.Exists($"players.{content.Replace("-", "").ToLower()}");
                }
                case "hasMod": {
                    if (reversed) throw new SyntaxErrorException("Has mod operator do not allow reversing");
                    return Builders<Server>.Filter.ElemMatch(x => x.Ping.LegacyForgeMods!.ModList, x => x.ModId == content) | 
                           Builders<Server>.Filter.ElemMatch(x => x.Ping.ModernForgeMods!.ModList, x => x.ModId == content);
                }
                default: throw new SyntaxErrorException(
                    $"Invalid operator \"{op}\"");
            }
        }, x => x.Ping.CleanDescription!);

    /// <summary>
    /// The shared portion of advanced query language
    /// </summary>
    /// <param name="query">Query String</param>
    /// <param name="handler">Operator Handler</param>
    /// <param name="def">Default Field</param>
    /// <returns>BSON filter</returns>
    private static FilterDefinition<T> Generate<T>(string query, 
        Func<string, bool, string, FilterDefinition<T>> handler,
        Expression<Func<T, object>> def) {
        var buffer = ""; var multi = false;
        var filter = Builders<T>.Filter.Empty;
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
            filter &= handler(op, reversed, content);
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
            filter &= Builders<T>.Filter.Regex(def,
                new BsonRegularExpression(clean[1..], "im"));
        return filter;
    }
}