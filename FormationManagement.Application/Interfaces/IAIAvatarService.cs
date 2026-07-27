namespace FormationManagement.Application.Interfaces;

/// <summary>Result of asking the avatar to speak/present a piece of content.</summary>
public class AvatarResponse
{
    /// <summary>URL of the generated talking-avatar video, if the provider is asynchronous/video-based (e.g. D-ID, HeyGen).</summary>
    public string? VideoUrl { get; set; }

    /// <summary>Direct streaming/session URL, if the provider supports live streaming avatars (e.g. Anam AI, Azure Avatar real-time).</summary>
    public string? StreamUrl { get; set; }

    /// <summary>The text the avatar is saying / answering, always populated so the UI can show captions even before video is ready.</summary>
    public string ResponseText { get; set; } = string.Empty;

    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Reusable, provider-agnostic abstraction over an external "AI avatar" service
/// (HeyGen, D-ID, Anam AI, Azure Avatar, ...). We deliberately do NOT build any
/// AI/video-generation logic ourselves — implementations only call the chosen
/// provider's REST API. Swapping providers means writing one new class that
/// implements this interface and changing one line in DI registration.
/// </summary>
public interface IAIAvatarService
{
    /// <summary>
    /// Asks the avatar to present a lesson's content (e.g. read out the lesson
    /// script/summary). Called when a learner opens a lesson page.
    /// </summary>
    Task<AvatarResponse> PresentLessonAsync(string lessonTitle, string lessonScript, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a learner's free-text question to the avatar (backed by the
    /// provider's conversational/LLM layer) and returns a spoken answer.
    /// </summary>
    Task<AvatarResponse> AnswerQuestionAsync(string question, string lessonContext, CancellationToken cancellationToken = default);
}
