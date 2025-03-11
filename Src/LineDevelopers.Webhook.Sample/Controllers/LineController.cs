using Line;
using Line.Message;
using Line.Webhook;
using Microsoft.AspNetCore.Mvc;

namespace LineDevelopers.Webhook.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineController : LineControllerBase
    {
        public LineController() : base(channelId: "", channelSecret: "")
        {
        }

        protected override async Task OnMessageEventAsync(IMessageClient client, MessageEventObject messageEventObject)
        {
            var messages = new List<IMessage>()
            {
                new TextMessage($"hello world!"),
            };

            switch (messageEventObject.Source)
            {
                case UserSource:
                    var user = (UserSource)messageEventObject.Source;
                    var userId = user.UserId;
                    break;
                case GroupChatSource:
                    var group = (GroupChatSource)messageEventObject.Source;
                    var groupID = group.GroupId;
                    var groupUserID = group.UserId;
                    break;
                case MultiPersonChatSource:
                    var multi = (MultiPersonChatSource)messageEventObject.Source;
                    var multiRoomId = multi.RoomId;
                    var multiUserId = multi.UserId;
                    break;
            }

            try
            {
                await client.SendReplyMessageAsync(messageEventObject.ReplyToken, messages);
            }
            catch (LineException ex)
            {
                var message = ex.Message;

                foreach (var detail in ex.Details ?? Enumerable.Empty<Detail>())
                {
                    var detailMessage = detail.Message;
                    var detailProperty = detail.Property;
                }
            }
        }

        protected override Task OnUnSendEventAsync(IMessageClient client, UnSendEventObject unSendEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnFollowEventAsync(IMessageClient client, FollowEventObject followEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnUnFollowEventAsync(IMessageClient client, UnFollowEventObject unFollowEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnJoinEventAsync(IMessageClient client, JoinEventObject joinEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnLeaveEventAsync(IMessageClient client, LeaveEventObject leaveEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnMemberJoinEventAsync(IMessageClient client,
            MemberJoinEventObject memberJoinEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnMemberLeaveEventAsync(IMessageClient client,
            MemberLeaveEventObject memberLeaveEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnPostBackEventAsync(IMessageClient client, PostBackEventObject postBackEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnVideoViewingCompleteEventAsync(IMessageClient client,
            VideoViewingCompleteEventObject videoViewingCompleteEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnBeaconEventAsync(IMessageClient client, BeaconEventObject beaconEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnAccountLinkEventAsync(IMessageClient client,
            AccountLinkEventObject accountLinkEventObject)
        {
            throw new NotImplementedException();
        }

        protected override Task OnThingsEventAsync(IMessageClient client, ThingsEventObject thingsEventObject)
        {
            throw new NotImplementedException();
        }
    }
}