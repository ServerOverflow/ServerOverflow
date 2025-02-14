namespace ServerOverflow.Frontend.Models;

/// <summary>
/// Generic pagination model
/// </summary>
public class SearchModel : StatusModel {
    /// <summary>
    /// List of items that matched the query
    /// </summary>
    public List<object>? Items { get; set; }
    
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