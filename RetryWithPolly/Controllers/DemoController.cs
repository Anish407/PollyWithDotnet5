using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RetryWithPolly.Controllers
{
   
    [ApiController]
    [Route("[controller]")]
    public class DemoController:ControllerBase
    {
        static int Count;
        [HttpGet]
        public IActionResult Get()
        {
            Count++;

            if (Count % 9 != 0) return BadRequest(Count);

            return Ok("hellloooooooooo");
        }

    }
}
