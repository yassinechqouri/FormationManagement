using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FormationManagement.Application.Common.Models;
using FormationManagement.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace FormationManagement.Infrastructure.Services.AIAvatar;

/// <summary>
/// Talks to the HeyGen "Streaming/Video Avatar" REST API. This is the only
/// class in the whole solution that knows about HeyGen's request/response
/// shape — everything else depends on <see cref="IAIAvatarService"/> only,
/// so switching to D-ID/Anam AI/Azure Avatar means writing a sibling class
/// (see DIdAvatarService, AnamAvatarService) and flipping the "Provider"
/// setting in appsettings.json + Program.cs registration.
/// </summary>
public class HeyGenAvatarService : IAIAvatarService
{
    private readonly HttpClient _httpClient;
    private readonly AIAvatarOptions _options;

    public HeyGenAvatarService(HttpClient httpClient, IOptions<AIAvatarOptions> options)
    {
        _options = options.Value;

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<AvatarResponse> PresentLessonAsync(string lessonTitle, string lessonScript, CancellationToken cancellationToken = default)
    {
        // HeyGen's "generate video" endpoint: POST /v2/video/generate
        // Docs: https://docs.heygen.com/reference/create-an-avatar-video-v2
        var payload = new
        {
            video_inputs = new[]
            {
                new
                {
                    character = new { type = "avatar", avatar_id = _options.AvatarId },
                    voice = new { type = "text", input_text = lessonScript, voice_id = _options.VoiceId }
                }
            },
            title = lessonTitle
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("v2/video/generate", payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new AvatarResponse
                {
                    Success = false,
                    ErrorMessage = $"HeyGen returned {(int)response.StatusCode}: {body}",
                    ResponseText = lessonScript
                };
            }

            using var doc = JsonDocument.Parse(body);

            // HeyGen returns a video_id; the actual mp4 URL becomes available
            // once processing finishes (poll GET /v1/video_status.get?video_id=...).
            var videoId = doc.RootElement.GetProperty("data").GetProperty("video_id").GetString();

            return new AvatarResponse
            {
                ResponseText = lessonScript,
                VideoUrl = $"{_options.BaseUrl}v1/video_status.get?video_id={videoId}",
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new AvatarResponse { Success = false, ErrorMessage = ex.Message, ResponseText = lessonScript };
        }
    }

    public async Task<AvatarResponse> AnswerQuestionAsync(string question, string lessonContext, CancellationToken cancellationToken = default)
    {
        // For live Q&A, HeyGen's "Streaming Avatar" (WebRTC) session is normally
        // driven from the browser SDK. Here we ask HeyGen's interactive-avatar
        // "task" endpoint to speak a pre-computed answer; in a full deployment
        // the answer text itself would come from your own LLM call using
        // `lessonContext` as grounding, then just be forwarded to HeyGen to speak.
        var answerText = $"Based on this lesson, here's the answer to \"{question}\": " +
                          "(plug your LLM/RAG answer generation in here before forwarding to the avatar).";

        var payload = new
        {
            avatar_id = _options.AvatarId,
            voice_id = _options.VoiceId,
            text = answerText
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("v1/streaming.task", payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            return new AvatarResponse
            {
                ResponseText = answerText,
                StreamUrl = _options.BaseUrl + "v1/streaming.task",
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new AvatarResponse { Success = false, ErrorMessage = ex.Message, ResponseText = answerText };
        }
    }
}
