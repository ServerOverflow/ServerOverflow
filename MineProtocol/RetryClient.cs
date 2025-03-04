namespace MineProtocol; 

/// <summary>
/// HTTP client with automatic retry on failure
/// </summary>
public class RetryClient : HttpClient {
    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    private const int _maxRetries = 5;
    
    /// <summary>
    /// Creates a new auto retry HTTP client
    /// </summary>
    /// <param name="handler">HTTP message handler</param>
    public RetryClient(HttpMessageHandler handler) : base(handler) { }

    /// <summary>
    /// Send an HTTP request as an asynchronous operation.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>HTTP response message</returns>
    public override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) {
        var attempts = 0; var delay = 1000;
        while (true) {
            try {
                var resp = await base.SendAsync(
                    Clone(request), cancellationToken);
                resp.EnsureSuccessStatusCode(); return resp;
            } catch (Exception e) {
                if (e is TaskCanceledException or HttpRequestException)
                    delay += 2000;
                await Task.Delay(delay, cancellationToken);
                attempts++;
                if (attempts > _maxRetries)
                    throw;
            }
        }
    }

    /// <summary>
    /// Clones an HTTP request message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>HTTP request message</returns>
    public static HttpRequestMessage Clone(HttpRequestMessage message)
        => new() {
            RequestUri = message.RequestUri,
            Content = message.Content,
            Method = message.Method
        };
}
