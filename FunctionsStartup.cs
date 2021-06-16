using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(RxBufferedFunctions.Startup))]

namespace RxBufferedFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ReactiveBufferService<DataItem>>();
            // builder.Services.AddHttpClient();

            // builder.Services.AddSingleton<IMyService>((s) => {
            //     return new MyService();
            // });

        }
    }
}