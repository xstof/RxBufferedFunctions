using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ReactiveWebApp.Controllers
{
    [ApiController]
    [Route("noop")]
    public class NoOpController : ControllerBase
    {
        private readonly ILogger<NoOpController> _logger;
        public NoOpController(ILogger<NoOpController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return new JsonResult(new { Status = "ok" });
        }

        [HttpPost]
        public ActionResult Post([FromBody] JsonElement content)
        {
            return new OkObjectResult(new { Status = "ok" });
        }
    }
}
