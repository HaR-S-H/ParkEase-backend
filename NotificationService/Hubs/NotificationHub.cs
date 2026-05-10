using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var recipientIdValue = Context.GetHttpContext()?.Request.Query["recipientId"].ToString();
            if (int.TryParse(recipientIdValue, out var recipientId) && recipientId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupNameForRecipient(recipientId));
            }

            await base.OnConnectedAsync();
        }

        public static string GroupNameForRecipient(int recipientId) => $"recipient:{recipientId}";
    }
}
