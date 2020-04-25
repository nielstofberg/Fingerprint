using FingerprintCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FingerPrintApi.Services
{
    public class FingerprintService : IFingerprintService
    {
        private Fingerprint _fp;

        public Fingerprint Fingerprint { get { return _fp; } }

        public FingerprintService()
        {
            _fp = new Fingerprint();
        }
    }
}
