namespace FormationManagement.Application.Common.Models;

/// <summary>
/// Bound from the "AIAvatar" section of appsettings.json. Holds only
/// provider-agnostic settings (which provider is active, its base URL/key)
/// — never hardcoded in code, so swapping HeyGen for D-ID etc. is config-only.
/// </summary>
public class AIAvatarOptions
{
    public const string SectionName = "AIAvatar";

    /// <summary>Which provider implementation to use: "HeyGen", "DId", "AnamAI", "AzureAvatar".</summary>
    public string Provider { get; set; } = "HeyGen";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Provider-specific avatar/persona identifier (e.g. HeyGen avatar_id).</summary>
    public string AvatarId { get; set; } = string.Empty;

    /// <summary>Provider-specific voice identifier.</summary>
    public string VoiceId { get; set; } = string.Empty;
}
