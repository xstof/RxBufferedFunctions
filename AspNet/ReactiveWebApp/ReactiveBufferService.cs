using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ReactiveWebApp{

    public class DeviceStatus {
            public string DeviceId { get; set; }
            public string Status { get; set;  }
            public string Channel { get; set; }

            public override string ToString()
            {
                return $"===> Device {DeviceId} is now {Status} on channel {Channel}";
            }
    }

    public class ViewerCount{
        public int CountOfViewers { get; set; }
        public string Channel { get; set; }
    }

    public interface IStreamViewerCounts{
        public IObservable<ViewerCount> GetStreamOfViewerCounts(int interval);
    }

    public class ReactiveBufferService<T> : IStreamViewerCounts where T: class {

        private readonly ILogger<ReactiveBufferService<T>> logger;
        private ConcurrentQueue<T> queue;
        private IObservable<T> stream;
        private IObserver<T> observer = null;
        private bool isInitialized = false;

        public ReactiveBufferService(ILogger<ReactiveBufferService<T>> logger){
            
            this.logger = logger;
            this.queue = new ConcurrentQueue<T>();

            logger.LogInformation("constructing ReactiveBufferService");

            // Initialize inner Observable - at this point there are no Observers yet so, we don't have an instance of Observer to push data towards
            var innerObservable = initStream();
            logger.LogInformation("initialized inner-observable - no subscriptions yet");

            // Subscribe to the inner Observable ourselves and make sure that subscription is shared by anyone else
            var connectedObservable           = innerObservable.Publish();          // Ensure subscription is shared amongst all Observers
            logger.LogInformation("published inner-observable");
            var connectedObservableDisposable = connectedObservable.Connect();      // Connect right away so all subscribers as of now get the same data pushed
            stream = connectedObservable;
            logger.LogInformation("connected inner-observable");
            logger.LogInformation("constructed ReactiveBufferService");

            // TODO: dispose of connectedObservable when stream needs to stop pushing data
            //logStreamAll(connectedObservable);
            //logFailedHeartBeatsAcrossChannels(connectedObservable);
            //logStatusChangesRegardlessOfChannel(connectedObservable);
            //logViewersByChannel(connectedObservable);
            //logViewersByChannelOnceEveryXSeconds(connectedObservable, 5);
        }

        public void Store(T item){
            if (isInitialized)
            {
                //observer.OnNext(item);
                queue.Enqueue(item);
            }
        }

        private void logStreamAll(IObservable<T> stream){
            logger.LogInformation("setting up stream to log every single item");
            stream.Do(item =>
            {
                //Console.WriteLine($"Item received: {item.ToString()}");
                logger.LogInformation($"==> item received: {item.ToString()}");
            })
            .Subscribe(); // subscribe for side effects only.
            logger.LogInformation("stream subscribed to");
        }

        private void logFailedHeartBeatsAcrossChannels(IObservable<T> stream){
            failedHeartbeatsRegardlessOfChannel(stream)
            .Do(item =>
                        {
                            logger.LogInformation(item);
                        })
                        .Subscribe(); // subscribe for side effects only.
        }

        private void logStatusChangesRegardlessOfChannel(IObservable<T> stream){
            statusChangesRegardlessOfChannel(stream)
            .Do(item =>
            {
                logger.LogInformation(item.ToString());
                //Console.WriteLine(item.ToString());
            })
            //.ObserveOn(System.Reactive.Concurrency.NewThreadScheduler.Default)
            .Subscribe();
        }

        private void logViewersByChannel(IObservable<T> stream){
            viewersByChannel(stream).Select(_ => $"===> Channel {_.Channel} has now {_.CountOfViewers} viewers")
            .Do(item =>
            {
                logger.LogInformation(item);
            })
            .Subscribe();
        }

        private void logViewersByChannelOnceEveryXSeconds(IObservable<T> stream, int interval){
            logger.LogInformation($"setting up stream to log viewers by channel every {interval} seconds");

            var viewerHeartBeats = stream.Cast<DataItem>();
            var viewerHeartBeatsByChannel = viewerHeartBeats.GroupBy(_ => _.Channel);
            var statusChangesByChannel = viewerHeartBeatsByChannel.Select(channelGroupedHeartBeats => statusChangesRegardlessOfChannel(channelGroupedHeartBeats.Cast<T>()));
            var continuousViewerCountByChannel = statusChangesByChannel.Select(statusStream => statusStream.Scan(new ViewerCount(){CountOfViewers = 0}, 
                                                                                                                (acc, status) => {
                                                                                                                    int viewerCount = acc.CountOfViewers + (status.Status == "Online" ? 1 : -1);
                                                                                                                    return new ViewerCount() { CountOfViewers = viewerCount, Channel = status.Channel };
                                                                                                                }));
            continuousViewerCountByChannel.Select(_ => _.Window(TimeSpan.FromSeconds(interval))
                                                        .Select(_ => _.TakeLast(1))
                                                        .Merge()
                                                        .Select(_ => $"===> Channel {_.Channel} has now {_.CountOfViewers} viewers."))
                                          .Merge()
            .ObserveOn(new EventLoopScheduler())
            .Do(item =>
            {
                logger.LogInformation($"[on thread {System.Threading.Thread.CurrentThread.ManagedThreadId} {item}]");
            })
            .Subscribe();
        }

        private IObservable<string> failedHeartbeatsRegardlessOfChannel(IObservable<T> stream, int expectedHeartBeatIntervalInSeconds = 10, int maxMissedHeartbeats = 1) {
            logger.LogInformation("setting up stream to detect failed heartbeat");
            // for info on technique, see here: https://blog.niallconnaughton.com/2016/03/21/service-health-monitoring-with-reactive-extensions/

            var dataItemStream = stream.Cast<DataItem>();                       // Cast to strongly typed stream of data items
            return dataItemStream.GroupBy(dataItem => dataItem.DeviceId)        // Group stream by device id
                                 .Select(groupedObservable => groupedObservable.Throttle(TimeSpan.FromSeconds(expectedHeartBeatIntervalInSeconds))
                                                                               .Select(_ => $"==> missed hearbeat from: {_.DeviceId} on channel {_.Channel}"))
                                 .Merge();
        }

        private IObservable<DeviceStatus> statusChangesRegardlessOfChannel(IObservable<T> stream, int expectedHeartBeatIntervalInSeconds = 10, int maxMissedHeartbeats = 1) {
            logger.LogInformation("setting up stream to calculate viewer status across channels");

            // all status update for devices based on their heartbeats
            var viewerHeartBeats = stream.Cast<DataItem>()
                                         .Select(_ => new DeviceStatus(){ DeviceId = _.DeviceId, Status = "Online", Channel = _.Channel });

            // all status update for devices based on their missed heartbeats
            var viewerMissedHeartBeats = 
                viewerHeartBeats.GroupBy(dataItem => dataItem.DeviceId)        // Group stream by device id
                                                    .Select(groupedObservable => groupedObservable.Throttle(TimeSpan.FromSeconds(expectedHeartBeatIntervalInSeconds)))
                                                    .Merge()
                                                    .Select(_ => new DeviceStatus() {DeviceId = _.DeviceId, Status = "Offline", Channel = _.Channel });

            // single stream of status updates with both notifications for hearbeat and missed heartbeats
            var combinedStatusStreamForAllViewers =
                viewerHeartBeats.Merge(viewerMissedHeartBeats);
            
            // cleaned up stream, omitting the duplicate heartbeats
            var combinedStatusStreamForAllViewersDistinctUntilChanged = 
                combinedStatusStreamForAllViewers.GroupBy(_ => _.DeviceId)
                                                 .Select(deviceStatusFeed => deviceStatusFeed.DistinctUntilChanged(_ => _.Status))
                                                 .Merge();

            return combinedStatusStreamForAllViewersDistinctUntilChanged;
        }

        private IObservable<ViewerCount> viewersByChannel(IObservable<T> stream, int expectedHeartBeatIntervalInSeconds = 10, int maxMissedHeartbeats = 1){
            logger.LogInformation("setting up stream to aggregate viewers by channel");

            var viewerHeartBeats = stream.Cast<DataItem>();
            var viewerHeartBeatsByChannel = viewerHeartBeats.GroupBy(_ => _.Channel);
            var statusChangesByChannel = viewerHeartBeatsByChannel.Select(channelGroupedHeartBeats => statusChangesRegardlessOfChannel(channelGroupedHeartBeats.Cast<T>()));
            var continuousViewerCountByChannel = statusChangesByChannel.Select(statusStream => statusStream.Scan(new ViewerCount(){CountOfViewers = 0}, 
                                                                                                                (acc, status) => {
                                                                                                                    int viewerCount = acc.CountOfViewers + (status.Status == "Online" ? 1 : -1);
                                                                                                                    return new ViewerCount() { CountOfViewers = viewerCount, Channel = status.Channel };
                                                                                                                }));
            var mergedViewerCountChanges = continuousViewerCountByChannel.Merge();
                                                                        //.Select(_ => $"===> Channel {_.Channel} has now {_.CountOfViewers} viewers");
            return mergedViewerCountChanges;
        }



        /// <summary>
        /// Initializes the RX stream by creating an Observable.
        /// </summary>
        /// <returns>
        /// Returns an inner-Observable which can be subscribed to.
        /// </returns>
        /// <remarks>
        /// Note: the Observer will only become available when the Observable is subscribed.
        /// </remarks>
        private IObservable<T> initStream(){
            logger.LogInformation("initializing stream");
            var o = Observable.Create<T>((IObserver<T> observer, CancellationToken token) =>
            {
                logger.LogInformation("creating observable in initStream");
                return Task.Run(() =>
                {
                    T nextItem;
                    while (! token.IsCancellationRequested){
                        if(queue.TryDequeue(out nextItem)){
                            observer.OnNext(nextItem);
                        }
                        else{
                            Thread.Sleep(100);
                        }
                    }
                }, token);
            });

            isInitialized = true;

            return o.ObserveOn(NewThreadScheduler.Default);
            //return o;
        }

        public IObservable<ViewerCount> GetStreamOfViewerCounts(int intervalInSeconds)
        {
            logger.LogInformation($"Getting stream of viewer counts for an interval of {intervalInSeconds} seconds");

            var viewerHeartBeats = stream.Cast<DataItem>();
            var viewerHeartBeatsByChannel = viewerHeartBeats.GroupBy(_ => _.Channel);
            var statusChangesByChannel = viewerHeartBeatsByChannel.Select(channelGroupedHeartBeats => statusChangesRegardlessOfChannel(channelGroupedHeartBeats.Cast<T>()));
            var continuousViewerCountByChannel = statusChangesByChannel.Select(statusStream => statusStream.Scan(new ViewerCount(){CountOfViewers = 0}, 
                                                                                                                (acc, status) => {
                                                                                                                    int viewerCount = acc.CountOfViewers + (status.Status == "Online" ? 1 : -1);
                                                                                                                    return new ViewerCount() { CountOfViewers = viewerCount, Channel = status.Channel };
                                                                                                                }));
            var streamOfViewerCounts = continuousViewerCountByChannel.Select(_ => _.Window(TimeSpan.FromSeconds(intervalInSeconds))
                                                        .Select(_ => _.TakeLast(1))
                                                        .Merge())
                                          .Merge()
                                          .ObserveOn(new EventLoopScheduler());
            return streamOfViewerCounts;
        }
    }

}