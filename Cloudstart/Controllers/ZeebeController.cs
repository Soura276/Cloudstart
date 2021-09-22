using Cloudstart.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cloudstart.Controllers
{
    public class ZeebeController : Controller
    {
        private readonly IZeebeService _zeebeService;

            public ZeebeController(IZeebeService zeebeService)
            {
                _zeebeService = zeebeService;
            }

            [Route("/status")]
            [HttpGet]
            public async Task<string> Get()
            {
                return (await _zeebeService.Status()).ToString();
            }

            [Route("/start")]
            [HttpGet]
            public async Task<string> StartWorkflowInstance()
            {
                var instance = await _zeebeService.StartWorkflowInstance("camunda-cloud-quick-start");
                return instance;
            }


    }
}
