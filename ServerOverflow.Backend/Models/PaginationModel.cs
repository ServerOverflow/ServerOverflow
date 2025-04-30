namespace ServerOverflow.Backend.Models;

/// <summary>
/// Generic pagination model
/// </summary>
public class PaginationModel<T> {
    /// <summary>
    /// List of items that matched the query
    /// </summary>
    public List<T>? Items { get; set; }
    
    /// <summary>
    /// The query itself
    /// </summary>
    public string? Query { get; set; }
    
    /// <summary>
    /// Current page
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Total number of query matches
    /// </summary>
    public long TotalMatches { get; set; }
}