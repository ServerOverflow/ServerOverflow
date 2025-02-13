namespace MineProtocol.Exceptions;

/// <summary>
/// Player disconnected exception
/// </summary>
/// <param name="message">Message</param>
public class DisconnectedException(string message) : Exception(message);