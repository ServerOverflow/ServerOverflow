using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace MineProtocol;

/// <summary>
/// Various useful extensions
/// </summary>
public static class Extensions {
    /// <summary>
    /// Reads a minecraft VarInt
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="timeout">Timeout</param>
    /// <returns>Integer</returns>
    public static async ValueTask<int> ReadVarInt(this Stream stream, TimeSpan? timeout = null) {
        timeout ??= TimeSpan.FromMilliseconds(-1);
        var value = 0;
        var size = 0;
        
        var buf = new byte[1];
        var read = 0;
        while (await stream.ReadAsync(buf.AsMemory(0, 1)).AsTask().WaitAsync(timeout.Value) > 0) {
            read++;
            if ((buf[0] & 0x80) != 0x80) break;
            value |= (buf[0] & 0x7F) << (size++ * 7);
            if (size > 5) throw new IOException("This VarInt is an imposter");
        }

        if (read == 0)
            throw new IOException("Failed to read VarInt");
        return value | ((buf[0] & 0x7F) << (size * 7));
    }
    
    /// <summary>
    /// Reads a string prefixed with VarInt
    /// </summary>
    /// <param name="stream">Stream</param>
    public static async Task<string> ReadString(this Stream stream) {
        var len = await stream.ReadVarInt();
        var buf = new byte[len];
        var read = await stream.ReadAsync(buf.AsMemory(0, len));
        return Encoding.UTF8.GetString(buf, 0, read);
    }
    
    /// <summary>
    /// Reads a boolean
    /// </summary>
    /// <param name="stream">Stream</param>
    public static async Task<bool> ReadBoolean(this Stream stream) {
        var buf = new byte[1];
        _ = await stream.ReadAsync(buf.AsMemory(0, 1));
        return buf[0] == 0x01;
    }
    
    /// <summary>
    /// Writes a minecraft VarInt
    /// </summary>
    /// <param name="stream">Binary Writer</param>
    /// <param name="value">Integer</param>
    public static async Task WriteVarInt(this Stream stream, int value) {
        var buf = new byte[5];
        if (value == 0) await stream.WriteAsync(buf.AsMemory(0, 1));
        var len = 0;
        while (value != 0) {
            var lol = value & 0x7F;
            value = (value >> 7) & (int.MaxValue >> 6);
            if (value != 0) lol |= 0b1000_0000;
            buf[len] = (byte)lol;
            len += 1;
        }
        
        await stream.WriteAsync(buf.AsMemory(0, len));
    }
    
    /// <summary>
    /// Writes a minecraft VarInt
    /// </summary>
    /// <param name="stream">Binary Writer</param>
    /// <param name="value">Integer</param>
    public static async Task WriteBoolean(this Stream stream, bool value) {
        var buf = new byte[1]; buf[0] = (byte)(value ? 1 : 0);
        await stream.WriteAsync(buf.AsMemory(0, 1));
    }

    /// <summary>
    /// Writes a string prefixed with VarInt
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="data">String</param>
    public static async Task WriteString(this Stream stream, string data) {
        var buffer = Encoding.UTF8.GetBytes(data);
        await stream.WriteVarInt(buffer.Length);
        await stream.WriteAsync(buffer);
    }
    
    /// <summary>
    /// Writes a byte buffer
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="buf">Buffer</param>
    public static async Task WriteBytes(this Stream stream, params byte[] buf)
        => await stream.WriteAsync(buf);

    /// <summary>
    /// Writes a short
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="value">Value</param>
    public static async Task WriteShort(this Stream stream, ushort value)
        => await stream.WriteBytes(BitConverter.GetBytes(value).Reverse().ToArray());
    
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