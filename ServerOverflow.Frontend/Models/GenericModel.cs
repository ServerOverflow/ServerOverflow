namespace ServerOverflow.Frontend.Models; 

/// <summary>
/// Generic model with a single object
/// </summary>
public class GenericModel<T> : StatusModel {
    /// <summary>
    /// Object instance
    /// </summary>
    public T? Item { get; set; }
}