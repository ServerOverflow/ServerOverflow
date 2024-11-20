using Serilog;
using ServerOverflow.Database;

namespace ServerOverflow.Processors;

/// <summary>
/// Profile processor
/// </summary>
public class Profiles {
    /// <summary>
    /// Main worker thread
    /// </summary>
    public static async Task MainThread() {
        while (true) {
            try {
                var profiles = await Profile.GetAll();
                foreach (var profile in profiles) {
                    try {
                        await profile.Refresh();
                        profile.Valid = true;
                    } catch (Exception e) {
                        Log.Warning("Failed to validate {0}: {1}",
                            profile.Username, e);
                        profile.Valid = false;
                    }

                    await profile.Update();
                }
                
                await Task.Delay(3600000);
            } catch (Exception e) {
                Log.Error("Profile refresher thread crashed: {0}", e);
            }
        }
    }
}