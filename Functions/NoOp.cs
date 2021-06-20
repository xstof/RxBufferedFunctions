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
    public class NoOp
    {
        public NoOp(){}

        [FunctionName("NoOp")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest request,
            ILogger log)
        {
            string responseMessage = 
                $"This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
