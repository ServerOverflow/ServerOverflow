using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Shared;

/// <summary>
/// Audit logs manager
/// </summary>
public static class Audit {
    public static async Task CreatedApiKey(Account perpetrator, ApiKey apiKey)
        => await LogEntry.Create(UserAction.CreatedApiKey, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_permissions"] = string.Join(", ", apiKey.Permissions),
            ["target_id"] = apiKey.Id.ToString(),
        });

    public static async Task UpdatedApiKey(Account perpetrator, ApiKey apiKey)
        => await LogEntry.Create(UserAction.UpdatedApiKey, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_permissions"] = string.Join(", ", apiKey.Permissions),
            ["target_id"] = apiKey.Id.ToString(),
        });

    public static async Task DeletedApiKey(Account perpetrator, ApiKey apiKey)
        => await LogEntry.Create(UserAction.DeletedApiKey, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_permissions"] = string.Join(", ", apiKey.Permissions),
            ["target_id"] = apiKey.Id.ToString(),
        });

    public static async Task CreatedInvitation(Account perpetrator, Invitation invite)
        => await LogEntry.Create(UserAction.CreatedInvitation, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_badge"] = invite.BadgeText,
            ["target_id"] = invite.Id.ToString()
        });

    public static async Task UpdatedInvitation(Account perpetrator, Invitation invite)
        => await LogEntry.Create(UserAction.UpdatedInvitation, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_badge"] = invite.BadgeText,
            ["target_id"] = invite.Id.ToString()
        });

    public static async Task DeletedInvitation(Account perpetrator, Invitation invite)
        => await LogEntry.Create(UserAction.DeletedInvitation, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_badge"] = invite.BadgeText,
            ["target_id"] = invite.Id.ToString()
        });

    public static async Task CreatedExclusion(Account perpetrator, Exclusion exclusion)
        => await LogEntry.Create(UserAction.CreatedExclusion, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_ips"] = exclusion.Count().ToString(),
            ["target_id"] = exclusion.Id.ToString()
        });

    public static async Task UpdatedExclusion(Account perpetrator, Exclusion exclusion)
        => await LogEntry.Create(UserAction.UpdatedExclusion, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_ips"] = exclusion.Count().ToString(),
            ["target_id"] = exclusion.Id.ToString()
        });

    public static async Task DeletedExclusion(Account perpetrator, Exclusion exclusion)
        => await LogEntry.Create(UserAction.DeletedExclusion, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_ips"] = exclusion.Count().ToString(),
            ["target_id"] = exclusion.Id.ToString()
        });

    public static async Task CreatedProfile(Account perpetrator, Profile profile)
        => await LogEntry.Create(UserAction.CreatedProfile, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_username"] = profile.Instance.Username,
            ["target_uuid"] = profile.Instance.UUID,
            ["target_id"] = profile.Id.ToString(),
        });

    public static async Task DeletedProfile(Account perpetrator, Profile profile)
        => await LogEntry.Create(UserAction.DeletedProfile, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_username"] = profile.Instance.Username,
            ["target_uuid"] = profile.Instance.UUID,
            ["target_id"] = profile.Id.ToString(),
        });

    public static async Task DeletedAccount(Account perpetrator, Account account)
        => await LogEntry.Create(UserAction.DeletedAccount, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_id"] = account.Id.ToString(),
            ["target_name"] = account.Username,
            ["target_badge"] = account.BadgeText
        });

    public static async Task UpdatedAccount(Account perpetrator, Account account)
        => await LogEntry.Create(UserAction.UpdatedAccount, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_id"] = account.Id.ToString(),
            ["target_name"] = account.Username,
            ["target_badge"] = account.BadgeText
        });

    public static async Task Registered(Account perpetrator)
        => await LogEntry.Create(UserAction.Registered, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText
        });

    public static async Task LoggedIn(Account perpetrator)
        => await LogEntry.Create(UserAction.LoggedIn, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText
        });

    public static async Task SearchedServers(Account perpetrator, string query, int page = 0, int pages = 0, long total = 0)
        => await LogEntry.Create(UserAction.SearchedServers, new Dictionary<string, string> {
            ["perpetrator_id"] = perpetrator.Id.ToString(),
            ["perpetrator_name"] = perpetrator.Username,
            ["perpetrator_badge"] = perpetrator.BadgeText,
            ["target_results"] = total.ToString(),
            ["target_pages"] = pages.ToString(),
            ["target_page"] = page.ToString(),
            ["target_query"] = query
        });
}

/// <summary>
/// User action type
/// </summary>
public enum UserAction {
    CreatedApiKey = 0,
    UpdatedApiKey = 1,
    DeletedApiKey = 2,
    CreatedInvitation = 3,
    UpdatedInvitation = 4,
    DeletedInvitation = 5,
    CreatedExclusion = 6,
    UpdatedExclusion = 7,
    DeletedExclusion = 8,
    CreatedProfile = 9,
    DeletedProfile = 10,
    DeletedAccount = 12,
    UpdatedAccount = 13,
    Registered = 14,
    LoggedIn = 15,
    SearchedServers = 16,
}
