using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MineProtocol;

/// <summary>
/// Various useful extensions
/// </summary>
public static class Extensions {
    /// <summary>
    /// Reads a minecraft VarInt
    /// </summary>
    /// <param name="reader">Binary Reader</param>
    /// <returns>Integer</returns>
    public static async Task<int> ReadVarInt(this AsyncBinaryReader reader) {
        var value = 0;
        var size = 0;
        int b;
        while (((b = await reader.ReadByteAsync()) & 0x80) == 0x80) {
            value |= (b & 0x7F) << (size++ * 7);
            if (size > 5) 
                throw new IOException("This VarInt is an imposter!");
        }

        return value | ((b & 0x7F) << (size * 7));
    }
    
    /// <summary>
    /// Writes a minecraft VarInt
    /// </summary>
    /// <param name="writer">Binary Writer</param>
    /// <param name="value">Integer</param>
    public static async Task WriteVarInt(this AsyncBinaryWriter writer, int value) {
        if (value == 0) await writer.WriteAsync((byte)value);
        while (value != 0) {
            var lol = value & 0x7F;
            value = (value >> 7) & (int.MaxValue >> 6);
            if (value != 0)
                lol |= 0b1000_0000;
            await writer.WriteAsync((byte)lol);
        }
    }

    /// <summary>
    /// Writes a string prefixed with VarInt
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="data"></param>
    public static async Task WriteString(this AsyncBinaryWriter writer, string data) {
        var buffer = Encoding.UTF8.GetBytes(data);
        await writer.WriteVarInt(buffer.Length);
        await writer.WriteAsync(buffer);
    }
    
    /// <summary>
    /// Computers a minecraft SHA-1 digest
    /// </summary>
    /// <param name="input">Buffer</param>
    /// <returns>Digest string</returns>
    public static string MinecraftDigest(this byte[] input) {
        var hash = SHA1.HashData(input);
        Array.Reverse(hash); var b = new BigInteger(hash);
        if (b < 0) return "-" + (-b).ToString("x").TrimStart('0');
        return b.ToString("x").TrimStart('0');
    }
}