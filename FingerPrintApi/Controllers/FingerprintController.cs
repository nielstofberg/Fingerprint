using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using FingerprintApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FingerprintApi.Controllers
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
        public async Task<object> GetAsync(bool count=false)
        {
            List<object> ret = new List<object>();
            if (count )
            {
                Console.WriteLine("Get FP count");
                int s = await _fps.Fingerprint.GetTemplateCount();
                return s;
            }
            else
            {
                Console.WriteLine("Place finger on reader to identify");
                bool varify = await _fps.Fingerprint.VarifyPasswordAsync();
                if (!varify)
                {
                    Console.WriteLine("Password authentication failed");
                    return false;
                }

                var usr = await _fps.Fingerprint.IdentifyFingerprint();
                return usr;
            }
        }


        // POST: api/Fingerprint
        [HttpPost]
        public async Task<bool> Post(int step, int? id)
        {
            Console.WriteLine("POST: api/Fingerprint");
            List<object> ret = new List<object>();
            if (step == 1)
            {
                Console.WriteLine("Enroll step1");
                bool s = await _fps.Fingerprint.EnrollStep1();
                return s;
            }
            else if (step == 2)
            {
                if (id == null || id == 0 || id > 1000)
                {
                    return false;
                }
                bool s = await _fps.Fingerprint.EnrollStep2((int)id);
                return s;
            }
            return false;
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
