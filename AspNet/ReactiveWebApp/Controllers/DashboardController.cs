using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ReactiveWebApp.Controllers
{
    public class DashboardController : Controller {
        public IActionResult Index(){
            return View();
        }
    }
}