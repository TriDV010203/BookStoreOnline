using Microsoft.AspNetCore.SignalR;

namespace BookStoreOnline.MVC.Hubs
{
    /// <summary>
    /// Hub thông báo đơn hàng realtime cho Admin
    /// </summary>
    public class OrderNotificationHub : Hub
    {
        /// <summary>
        /// Admin join group "admin" khi kết nối
        /// </summary>
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }

        /// <summary>
        /// Admin rời group khi ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
