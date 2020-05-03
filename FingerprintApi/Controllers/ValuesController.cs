using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FingerprintApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FingerprintApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        IFingerprintService _fps;

        public ValuesController(IFingerprintService fps)
        {
            _fps = fps;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return FingerprintCore.Fingerprint.GetSerialPorts();
        }
    }
}
