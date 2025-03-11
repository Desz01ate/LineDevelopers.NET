using System.Runtime.CompilerServices;
using Line;
using Line.Message;

namespace LineDevelopers.Webhook.Sample.Extensions;

public static class LineHttpClientExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_httpClient")]
    private static extern ref HttpClient GetHttpClient(LineHttpClient client);

    public static Task<byte[]> GetContentAsync(this IMessageClient client, string messageId)
    {
        LineHttpClient lineHttpClient = (LineMessageClient)client;

        return GetContentAsync(lineHttpClient, messageId);
    }

    /// <summary>
    /// https://developers.line.biz/en/reference/messaging-api/#getting-content
    /// The domain name (api-data.line.me) of this endpoint is for sending and receiving large amounts of data in the LINE Platform for Messaging API.
    /// This domain name differs from that of other endpoints (api.line.me).
    /// </summary>
    /// <returns></returns>
    private static async Task<byte[]> GetContentAsync(this LineHttpClient client, string messageId)
    {
        var endpoint = new Uri($"https://api-data.line.me/v2/bot/message/{messageId}/content");
        var httpClient = GetHttpClient(client);

        using var response = await httpClient.GetAsync(endpoint).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsByteArrayAsync();

        return result;
    }
}