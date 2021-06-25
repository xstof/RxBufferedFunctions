using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ReactiveWebApp
{
    public class SignalRDispatcherService
    {
        private readonly ILogger<SignalRDispatcherService> logger;
        private readonly IHubContext<SignalRHub> hub;
        private readonly IStreamViewerCounts viewerCountsStream;
        public SignalRDispatcherService(ILogger<SignalRDispatcherService> logger, IHubContext<SignalRHub> hub, IStreamViewerCounts viewerCountsStream){
            logger.LogInformation("constructing SignalR dispatching service");

            this.logger = logger;
            this.hub = hub;
            this.viewerCountsStream = viewerCountsStream;

            logger.LogInformation("constructed  SignalR dispatching service");
        }

        public void SubscribeToBufferService() {
            logger.LogInformation("subscribing to viewer counts from for consumption in SignalR dispatch service");

            var viewers = viewerCountsStream.GetStreamOfViewerCounts(2).ObserveOn(new EventLoopScheduler());
            viewers.Subscribe(async viewercount =>
            {
                logger.LogInformation("dispatching viewer count update");

                await hub.Clients.All.SendAsync("updateViewerCount", new {
                    Channel = viewercount.Channel,
                    ViewerCount = viewercount.CountOfViewers
                });
            });

            logger.LogInformation("subscribed to viewer counts from for consumption in SignalR dispatch service");
        }
    }

    public static class ApplicationBuilderExtensions{
         public static void UseSignalRDispatcher(this IApplicationBuilder app){
            var dispatcherService = (SignalRDispatcherService)app.ApplicationServices.GetService(typeof(SignalRDispatcherService));
            dispatcherService.SubscribeToBufferService();
        }
    }
}