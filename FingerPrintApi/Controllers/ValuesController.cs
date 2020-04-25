using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FingerPrintApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FingerPrintApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        IFingerprintService _fps;

        public ValuesController(IFingerprintService fps)
        {
            _fps = fps;
            if (!_fps.Fingerprint.IsInitialised)
            {
                _fps.Fingerprint.Init();
            }
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
