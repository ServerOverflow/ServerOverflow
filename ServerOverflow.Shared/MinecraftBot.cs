using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MineProtocol;
using MineProtocol.Exceptions;
using MineProtocol.Protocol;
using ServerOverflow.Shared.Storage;
using Profile = MineProtocol.Authentication.Profile;

namespace ServerOverflow.Shared;

/// <summary>
/// A simple bot that joins servers and checks if it has online mode or whitelist enabled
/// </summary>
public static class MinecraftBot {
    /// <summary>
    /// Offline mode profile
    /// </summary>
    private static readonly Profile _offline = new("ServerOverflow1", "91ebb6fa-6a5d-4d97-8647-aa4f8298f587");

    /// <summary>
    /// Web proxy to use for join requests to the API
    /// </summary>
    public static WebProxy? JoinProxy { get; set; }
    
    /// <summary>
    /// Joins a server and returns the result
    /// </summary>
    /// <param name="server">Server</param>
    /// <param name="profile">Profile</param>
    /// <returns>Result</returns>
    public static async Task<JoinResult> Join(Server server, Profile? profile = null)
        => await Join(server, profile, null, 0);

    /// <summary>
    /// Joins a server and returns the result
    /// </summary>
    /// <param name="server">Server</param>
    /// <param name="profile">Profile</param>
    /// <param name="protocol">Protocol</param>
    /// <param name="depth">Depth</param>
    /// <param name="retries">Retries</param>
    /// <returns>Result</returns>
    private static async Task<JoinResult> Join(Server server, Profile? profile, int? protocol, int depth, int retries = 3) {
        profile ??= _offline;
        try {
            if (depth > 3)
                throw new InvalidOperationException("Detected outdated client cycle");
            using var proto = new TinyClient(server.IP, server.Port,
                protocol ?? server.JoinResult?.RealProtocol ?? server.Ping.Version?.Protocol ?? 47,
                server.Ping.IsForge, server.Ping.ModernForgeMods?.ProtocolVersion ?? 0);
            await proto.Connect();
            await proto.Handshake();
            await proto.LoginStart(profile);
            while (true) {
                var packet = await proto.Receive();
                switch (packet.Id) {
                    case PacketId.EncryptionRequest:
                        if (profile.Minecraft != null) {
                            var serverId = await packet.Stream.ReadString();
                            var publicKeyLen = await packet.Stream.ReadVarInt();
                            var publicKey = new byte[publicKeyLen];
                            _ = packet.Stream.Read(publicKey, 0, publicKey.Length);
                            var verifyTokenLen = await packet.Stream.ReadVarInt();
                            var verifyToken = new byte[verifyTokenLen];
                            _ = packet.Stream.Read(verifyToken, 0, verifyToken.Length);
                            var secretKey = RandomNumberGenerator.GetBytes(16);
                            if (packet.Stream.Position == packet.Stream.Length || await packet.Stream.ReadBoolean())
                                try {
                                    await MojangAPI.JoinServer(profile, MojangAPI.GetServerId(serverId, secretKey, publicKey), JoinProxy);
                                } catch (Exception e) {
                                    throw new Exception("Failed to authenticate with Mojang", e);
                                }
                            await packet.Skip();
                            await proto.Encrypt(secretKey, publicKey, verifyToken);
                            break;
                        }
                        
                        proto.Disconnect();
                        return new JoinResult { 
                            RealProtocol = protocol ?? server.Ping.Version?.Protocol ?? 47,
                            Success = true, OnlineMode = true, LastSeen = DateTime.UtcNow,
                            Timestamp = DateTime.UtcNow
                        };
                    case PacketId.LoginSuccess:
                        proto.Disconnect();
                        return new JoinResult {
                            RealProtocol = protocol ?? server.Ping.Version?.Protocol ?? 47, 
                            Success = true, OnlineMode = profile.Minecraft != null,
                            Whitelist = false, LastSeen = DateTime.UtcNow,
                            Timestamp = DateTime.UtcNow
                        };
                    default:
                        await packet.Skip();
                        break;
                }
            }
        } catch (DisconnectedException e) {
            if (e.Message.Contains("1.13 and above"))
                return await Join(server, profile, 393, depth + 1, retries);
            var match = Regex.Match(e.Message, @"Outdated client! Please use (\d\.\d+\.\d+)");
            if (match.Success && Resources.Version.TryGetValue(match.Groups[1].Value, out var newProto))
                return await Join(server, profile, newProto, depth + 1, retries);
            if (retries > 0)
                return await Join(server, profile, protocol, depth, retries - 1);
            return new JoinResult {
                RealProtocol = protocol ?? server.Ping.Version?.Protocol ?? 47,
                Success = true, OnlineMode = profile.Minecraft != null,
                Whitelist = true, DisconnectReason = e.Message,
                LastSeen = DateTime.UtcNow, Timestamp = DateTime.UtcNow
            };
        } catch (Exception e) {
            return new JoinResult {
                Success = false, Timestamp = DateTime.UtcNow,
                ErrorMessage = e.Message, Exception = e.ToString()
            };
        }
    }
}
