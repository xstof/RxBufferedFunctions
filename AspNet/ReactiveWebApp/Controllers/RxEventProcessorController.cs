using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ReactiveWebApp.Controllers{

    [ApiController]
    [Route("api/ReceiveRaw")]
    public class RxEventProcessorController : ControllerBase{

        private readonly ILogger<RxEventProcessorController> logger;
        private readonly ReactiveBufferService<DataItem> buffer;

        public RxEventProcessorController(ILogger<RxEventProcessorController> logger,
                                          ReactiveBufferService<DataItem> buffer){
            this.logger = logger;
            this.buffer = buffer;
        }

        [HttpPost]
        public ActionResult Post([FromBody] DataItem dataItem)
        {
            buffer.Store(dataItem);

            return new OkObjectResult(new { Status = "ok" });
        }
    }
}