using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NotificacionesAPI.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string companyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
        }

        public async Task LeaveGroup(string companyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, companyId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}