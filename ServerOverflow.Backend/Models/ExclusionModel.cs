using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Models;

/// <summary>
/// Exclusion model
/// </summary>
public class ExclusionModel {
    /// <summary>
    /// Exclusion identifier
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// List of IP ranges to exclude
    /// </summary>
    public List<string>? Ranges { get; set; }

    /// <summary>
    /// Comment for this exclusion
    /// </summary>
    public string? Comment { get; set; }
    
    /// <summary>
    /// Empty constructor for JSON
    /// </summary>
    public ExclusionModel() {}

    /// <summary>
    /// Creates a new user model
    /// </summary>
    /// <param name="exclusion">Exclusion</param>
    public ExclusionModel(Exclusion exclusion) {
        Id = exclusion.Id.ToString();
        Comment = exclusion.Comment;
        Ranges = exclusion.Ranges;
    }
}