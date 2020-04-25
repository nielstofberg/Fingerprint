using FingerprintCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FingerPrintApi.Services
{
    public interface IFingerprintService
    {
        Fingerprint Fingerprint { get; }
    }
}
