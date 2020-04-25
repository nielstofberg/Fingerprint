using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using FingerPrintApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FingerPrintApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FingerprintController : ControllerBase
    {
        IFingerprintService _fps;

        public FingerprintController(IFingerprintService fps)
        {
            _fps = fps;
            if (!_fps.Fingerprint.IsInitialised)
            {
                _fps.Fingerprint.Init();
            }
        }

        // GET: api/Fingerprint
        [HttpGet]
        public async Task<object> GetAsync(int? step, int? id)
        {
            List<object> ret = new List<object>();
            if (step == 1)
            {
                bool s = await _fps.Fingerprint.EnrollStep1();
                ret.Add(s);
            }
            else if (step == 2)
            {
                if (id == null || id == 0 || id > 1000)
                {
                    id = await _fps.Fingerprint.GetTemplateCount() + 1;
                }
                bool s = await _fps.Fingerprint.EnrollStep2((int)id);
                ret.Add(s);
            }
            else
            {
                bool varify = await _fps.Fingerprint.VarifyPasswordAsync();
                if (!varify) return false;

                var usr = await _fps.Fingerprint.IdentifyFingerprint();
                return usr;
            }
            return false;
        }


        // GET: api/Fingerprint/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Fingerprint
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Fingerprint/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
