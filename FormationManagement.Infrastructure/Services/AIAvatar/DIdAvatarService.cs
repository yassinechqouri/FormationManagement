using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FormationManagement.Application.Common.Models;
using FormationManagement.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace FormationManagement.Infrastructure.Services.AIAvatar;

/// <summary>
/// Alternative provider implementation for D-ID (https://docs.d-id.com).
/// Demonstrates that <see cref="IAIAvatarService"/> is truly provider-agnostic:
/// to switch from HeyGen to D-ID, register this class instead in
/// DependencyInjection.cs and set AIAvatar:Provider = "DId" in appsettings.json
/// — no other file in the solution needs to change.
/// </summary>
public class DIdAvatarService : IAIAvatarService
{
    private readonly HttpClient _httpClient;
    private readonly AIAvatarOptions _options;

    public DIdAvatarService(HttpClient httpClient, IOptions<AIAvatarOptions> options)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);

        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(_options.ApiKey));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<AvatarResponse> PresentLessonAsync(string lessonTitle, string lessonScript, CancellationToken cancellationToken = default)
    {
        // D-ID "Talks" endpoint: POST /talks
        var payload = new
        {
            script = new { type = "text", input = lessonScript, provider = new { type = "microsoft", voice_id = _options.VoiceId } },
            source_url = _options.AvatarId // D-ID uses an image URL as the "avatar"
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("talks", payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Surface D-ID's actual error message (e.g. "source_url is not
                // accessible" or "invalid presenter") instead of just "400 Bad Request".
                return new AvatarResponse
                {
                    Success = false,
                    ErrorMessage = $"D-ID returned {(int)response.StatusCode}: {body}",
                    ResponseText = lessonScript
                };
            }

            using var doc = JsonDocument.Parse(body);
            var talkId = doc.RootElement.GetProperty("id").GetString();

            // The POST above only starts rendering — D-ID needs another
            // 10-30 seconds to actually produce the video file. Poll the
            // status endpoint until it reports "done" and hands back a real,
            // playable result_url (an mp4 hosted on D-ID's storage), rather
            // than pointing the <video> tag at the status-check API itself.
            const int maxAttempts = 20;
            const int delayMilliseconds = 3000;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                await Task.Delay(delayMilliseconds, cancellationToken);

                var statusResponse = await _httpClient.GetAsync($"talks/{talkId}", cancellationToken);
                var statusBody = await statusResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!statusResponse.IsSuccessStatusCode)
                {
                    return new AvatarResponse
                    {
                        Success = false,
                        ErrorMessage = $"D-ID status check returned {(int)statusResponse.StatusCode}: {statusBody}",
                        ResponseText = lessonScript
                    };
                }

                using var statusDoc = JsonDocument.Parse(statusBody);
                var status = statusDoc.RootElement.GetProperty("status").GetString();

                if (status == "done")
                {
                    var resultUrl = statusDoc.RootElement.GetProperty("result_url").GetString();
                    return new AvatarResponse
                    {
                        ResponseText = lessonScript,
                        VideoUrl = resultUrl,
                        Success = true
                    };
                }

                if (status == "error" || status == "rejected")
                {
                    var errorDetails = statusDoc.RootElement.TryGetProperty("error", out var errorProp)
                        ? errorProp.ToString()
                        : status;

                    return new AvatarResponse
                    {
                        Success = false,
                        ErrorMessage = $"D-ID video generation failed: {errorDetails}",
                        ResponseText = lessonScript
                    };
                }

                // status is "created" or "started" — still rendering, keep polling.
            }

            return new AvatarResponse
            {
                Success = false,
                ErrorMessage = "D-ID video took too long to render (timed out after ~60 seconds).",
                ResponseText = lessonScript
            };
        }
        catch (Exception ex)
        {
            return new AvatarResponse { Success = false, ErrorMessage = ex.Message, ResponseText = lessonScript };
        }
    }

    public Task<AvatarResponse> AnswerQuestionAsync(string question, string lessonContext, CancellationToken cancellationToken = default)
    {
        // D-ID's REST API is talk-generation only (no built-in live conversation
        // endpoint at time of writing) — a real Q&A flow would generate the
        // answer text with your own LLM call, then reuse PresentLessonAsync to
        // render it as a talking-avatar clip.
        var answerText = $"(Generate an answer to \"{question}\" using lesson context, then call PresentLessonAsync to render it.)";
        return Task.FromResult(new AvatarResponse { ResponseText = answerText, Success = true });
    }
}
