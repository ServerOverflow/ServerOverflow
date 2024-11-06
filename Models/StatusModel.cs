namespace ServerOverflow.Models; 

/// <summary>
/// Status message model
/// </summary>
public class StatusModel {
    /// <summary>
    /// Status message. Can be null if none.
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Is the operation successful
    /// </summary>
    public bool Success { get; set; }
}