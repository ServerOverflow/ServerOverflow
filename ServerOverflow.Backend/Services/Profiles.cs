using Serilog;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Services;

/// <summary>
/// Profile processor
/// </summary>
public class Profiles : BackgroundService {
    /// <summary>
    /// Runs the main service loop
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken token) {
        while (true) {
            try {
                var profiles = await Profile.GetAll();
                foreach (var profile in profiles) {
                    try {
                        await profile.Instance.Refresh();
                        profile.Valid = true;
                    } catch (Exception e) {
                        Log.Warning("Failed to validate {0}: {1}",
                            profile.Instance.Username, e);
                        profile.Valid = false;
                    }

                    await profile.Update();
                }
            } catch (OperationCanceledException) {
                break;
            } catch (Exception e) {
                Log.Error("Profile refresher thread crashed: {0}", e);
            }
            
            await Task.Delay(3600000, token);
        }
    }
}