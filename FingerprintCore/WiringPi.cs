using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FingerprintCore
{
    public class WiringPi
    {
        [DllImport("libwiringPi.so", EntryPoint = "serialOpen")]     //This is an example of how to call a method / function in a c library from c#
        private static unsafe extern int serialOpen(
                        [MarshalAs(UnmanagedType.LPArray)]  byte[] device,
                                                            int baud);

        [DllImport("libwiringPi.so", EntryPoint = "serialClose")]     //This is an example of how to call a method / function in a c library from c#
        private static unsafe extern void serialClose(int fd);


        public static bool SetBaudRate(string portName, int baud)
        {
            var pn = Encoding.ASCII.GetBytes(portName);
            var x = serialOpen(pn, baud);
            if (x < 0)
            {
                Console.WriteLine("Eppic fail trying to use Wiring pi to set baudrate");
                return false;
            }

            serialClose(x);
            return true;
        }
    }
}
