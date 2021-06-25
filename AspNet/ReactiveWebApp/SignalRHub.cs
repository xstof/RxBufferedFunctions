using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ReactiveWebApp {
    public class SignalRHub : Hub
    {
        public SignalRHub(){}
        public async Task UpdateViewerCount(string channel, int viewerCount){
            await this.Clients.All.SendAsync("updateViewerCount", new {
                Channel = channel,
                ViewerCount = viewerCount
            });
        }

    }
}