using FingerprintCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class FingerprintProtocolTest
    {
        [TestMethod]
        public void TestValidate()
        {

            byte[] packet;
            //                    [ start  ]  [           addr           ]  [ length ]  [cmd] [          DATA      ]  [ checksum]
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1B };

            //Bad Start Code
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x07, 0x00, 0x03, 0x01, 0x00, 0x0B };
            Assert.IsTrue(FingerPrintProtocol.ValidatePacket(packet));

            //Bad Start Code
            packet = new byte[] { 0xdf, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1b };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));

            //Bad Start Code
            packet = new byte[] { 0xef, 0x02, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1b };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));

            //Packet too short
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00 };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));

            //Packet too short with leading bytes
            packet = new byte[] { 1, 2, 3, 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13 };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));

            // Length byte too high
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x08, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1b };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));

            // Length byte too low
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x06, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1b };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));

            // Bad checksum
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06 };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));

            // Bad Checksum
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00, 0x00, 0x00, 0x00, 0x01, 0x1b };
            Assert.IsFalse(FingerPrintProtocol.ValidatePacket(packet));


            // Good Packet
            packet = new byte[] { 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1b };
            Assert.IsTrue(FingerPrintProtocol.ValidatePacket(packet));

            // Good with leading and trailing bytes;
            packet = new byte[] { 1, 2, 3, 0xef, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x07, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1b, 1, 2, 3 };
            Assert.IsTrue(FingerPrintProtocol.ValidatePacket(packet));


        }

        [TestMethod]
        public void TestParse()
        {
            var pack = new FingerPrintProtocol(FingerprintCommand.VERIFYPASSWORD, new byte[] { 5, 4, 8, 7,2,5,7,8,9,0,4,6,1,3,4,6,8,1,5,2 });

            var p1 = FingerPrintProtocol.Parse(pack.GetStructuredPacket());

            Assert.IsTrue(pack.Pid == p1.Pid);
            Assert.IsTrue(pack.Command == p1.Command);

            for (int n=0;n < p1.Data.Length; n++)
            {
                Assert.IsTrue(pack.Data[n] == p1.Data[n]);
            }
        }

    }
}
