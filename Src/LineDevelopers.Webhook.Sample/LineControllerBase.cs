using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Line;
using Line.Message;
using Line.Webhook;
using Microsoft.AspNetCore.Mvc;

namespace LineDevelopers.Webhook.Sample
{
    public abstract class LineControllerBase : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            Converters =
            {
                new CustomJsonStringEnumConverter(),
            }
        };

        private readonly string channelId;
        private readonly string channelSecret;

        protected LineControllerBase(string channelId, string channelSecret)
        {
            this.channelId = channelId;
            this.channelSecret = channelSecret;
        }

        [HttpPost("callback")]
        public async Task<IActionResult> CallBackAsync()
        {
            using var ms = new MemoryStream();
            await this.HttpContext.Request.Body.CopyToAsync(ms);
            var body = ms.ToArray();

            var headers = this.HttpContext.Request.Headers;

            if (!headers.TryGetValue("X-Line-Signature", out var sig))
            {
                return this.BadRequest();
            }

            var bodySignature = CalculateSignature(this.channelSecret, body);

            if (sig != bodySignature)
            {
                return this.BadRequest();
            }

            var request = JsonSerializer.Deserialize<WebhookBody>(body, JsonSerializerOptions)!;

            using var lcatClient = new LineChannelAccessTokenClient();
            var accessTokenResult =
                await lcatClient.IssueShortLivedChannelAccessTokenAsync(this.channelId, this.channelSecret);

            await lcatClient.VerifyShortLonglivedChannelAccessTokenAsync(accessTokenResult.AccessToken);

            using var client = new LineMessageClient(accessTokenResult.AccessToken);

            try
            {
                foreach (var currentEvent in request.Events)
                {
                    var task = currentEvent switch
                    {
                        MessageEventObject e => this.OnMessageEventAsync(client, e),
                        UnSendEventObject e => this.OnUnSendEventAsync(client, e),
                        FollowEventObject e => this.OnFollowEventAsync(client, e),
                        UnFollowEventObject e => this.OnUnFollowEventAsync(client, e),
                        JoinEventObject e => this.OnJoinEventAsync(client, e),
                        LeaveEventObject e => this.OnLeaveEventAsync(client, e),
                        MemberJoinEventObject e => this.OnMemberJoinEventAsync(client, e),
                        MemberLeaveEventObject e => this.OnMemberLeaveEventAsync(client, e),
                        PostBackEventObject e => this.OnPostBackEventAsync(client, e),
                        VideoViewingCompleteEventObject e => this.OnVideoViewingCompleteEventAsync(client, e),
                        BeaconEventObject e => this.OnBeaconEventAsync(client, e),
                        AccountLinkEventObject e => this.OnAccountLinkEventAsync(client, e),
                        ThingsEventObject e => this.OnThingsEventAsync(client, e),
                        _ => throw new NotSupportedException(),
                    };

                    await task.ConfigureAwait(false);
                }

                return Ok();
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static string CalculateSignature(string secret, byte[] body)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(body);
            var base64 = Convert.ToBase64String(hash);

            return base64;
        }

        protected abstract Task OnMessageEventAsync(IMessageClient client, MessageEventObject messageEventObject);
        protected abstract Task OnUnSendEventAsync(IMessageClient client, UnSendEventObject unSendEventObject);
        protected abstract Task OnFollowEventAsync(IMessageClient client, FollowEventObject followEventObject);
        protected abstract Task OnUnFollowEventAsync(IMessageClient client, UnFollowEventObject unFollowEventObject);
        protected abstract Task OnJoinEventAsync(IMessageClient client, JoinEventObject joinEventObject);
        protected abstract Task OnLeaveEventAsync(IMessageClient client, LeaveEventObject leaveEventObject);

        protected abstract Task OnMemberJoinEventAsync(IMessageClient client,
            MemberJoinEventObject memberJoinEventObject);

        protected abstract Task OnMemberLeaveEventAsync(IMessageClient client,
            MemberLeaveEventObject memberLeaveEventObject);

        protected abstract Task OnPostBackEventAsync(IMessageClient client, PostBackEventObject postBackEventObject);

        protected abstract Task OnVideoViewingCompleteEventAsync(
            IMessageClient client,
            VideoViewingCompleteEventObject videoViewingCompleteEventObject);

        protected abstract Task OnBeaconEventAsync(IMessageClient client, BeaconEventObject beaconEventObject);

        protected abstract Task OnAccountLinkEventAsync(IMessageClient client,
            AccountLinkEventObject accountLinkEventObject);

        protected abstract Task OnThingsEventAsync(IMessageClient client, ThingsEventObject thingsEventObject);
    }
}