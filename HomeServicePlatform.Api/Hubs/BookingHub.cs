using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Hubs
{
    public class BookingHub : Hub
    {
        public async Task StreamDriverLocation(string providerId, double latitude, double longitude)
        {
            await Clients.All.SendAsync("ReceiveDriverLocation", providerId, latitude, longitude);
        }
    }
}
