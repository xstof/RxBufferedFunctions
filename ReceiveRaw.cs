using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RxBufferedFunctions
{
    public class ReceiveRaw
    {

        public ReceiveRaw(){

        }

        [FunctionName("ReceiveRaw")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] DataItem receivedItem,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = 
                $"This HTTP triggered function executed successfully.  Device has id: {receivedItem.DeviceId}";

            return new OkObjectResult(responseMessage);
        }
    }
}
