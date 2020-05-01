using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FingerprintCore
{
    public class Fingerprint: IDisposable
    {
        List<byte> _inBuff = new List<byte>();
        SerialDevice _sps;
        private bool _wiating = false;
        private bool _newPacket = true;

        public bool IsInitialised { get; private set; }

        public string SerialPortName { get; set; } = "/dev/serial0";

        public Fingerprint()
        {

        }

        public void Init(string serialPort)
        {
            SerialPortName = serialPort;
        }

        public void Init()
        {
            bool validsp = false;
            if (SerialPortName.Length == 0) return;

            foreach (var str in GetSerialPorts())
            {
                if (str == SerialPortName)
                {
                    validsp = true;
                    break;
                }
            }
            if (!validsp) return;

            WiringPi.SetBaudRate(SerialPortName, 57600);

            //Selected serialport is valid
            _sps = new SerialDevice(SerialPortName, BaudRate.B57600);            
            _sps.DataReceived += Sps_DataReceived;
            try
            {
                _sps.Open();
            }
            catch (Exception)
            {
                return;
            }

            IsInitialised = true;
        }

        /// <summary>
        /// Data Received event handler
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Sps_DataReceived(object arg1, byte[] arg2)
        {
            string str = BitConverter.ToString(arg2);

            if (_wiating)
            {
                Console.WriteLine($"Received: {str}");

                _inBuff.AddRange(arg2);

                if (FingerPrintProtocol.ValidatePacket(_inBuff.ToArray()))
                {
                    _newPacket = true;
                }
            }
        }

        /// <summary>
        /// Authenticate before starting to use the Fingerprint reader.
        /// </summary>
        /// <returns></returns>
        public Task<bool> VarifyPasswordAsync()
        {
            try
            {
                SendCommand(FingerprintCommand.VERIFYPASSWORD, new byte[] { 0, 0, 0, 0 });
                var reply = GetReply();
                if (reply != null &&
                    reply.Pid == PacketIdentifier.ACKPACKET &&
                    reply.ConfCode == ConfirmationCode.ExeComplete)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    Console.WriteLine("bad reply");
                    return Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Varify Password Failed: {ex.Message}");
            }

            _wiating = false;
            return Task.FromResult(false);
        }

        /// <summary>
        /// Get the number of Templates stored on the sensor
        /// </summary>
        /// <returns></returns>
        public Task<int> GetTemplateCount()
        {
            try
            {
                SendCommand(FingerprintCommand.TEMPLATECOUNT, new byte[0]);
                var reply = GetReply();
                if (reply != null &&
                    reply.Pid == PacketIdentifier.ACKPACKET &&
                    reply.ConfCode == ConfirmationCode.ExeComplete)
                {
                    int ret = (reply.Data[0] << 8) | (reply.Data[1] & 0xff);
                    return Task.FromResult(ret);
                }
                else
                {
                    Console.WriteLine("bad reply");
                    return Task.FromResult(0);
                }
            }
            catch { }

            _wiating = false;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Create a new fingerprint
        /// </summary>
        /// <returns></returns>
        public Task<bool> EnrollStep1()
        {
            Console.WriteLine("Enroll step1");
            DateTime to = DateTime.Now.AddSeconds(5);
            while (DateTime.Now < to)
            {
                if (GetImage())
                {
                    if (Image2Tz(1))
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> EnrollStep2(int id)
        {
            Console.WriteLine("Enroll step2");
            DateTime to = DateTime.Now.AddSeconds(5);
            while (DateTime.Now < to)
            {
                if (GetImage())
                {
                    if (Image2Tz(2))
                    {
                        if (CreateModel())
                        {
                            if (StoreModel(id))
                            {
                                return Task.FromResult(true);
                            }
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// Read a user's fingerprint from the sensor
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>Structure that confirms a match, gives the id and match score</returns>
        public Task<FpMatch> IdentifyFingerprint(int timeout=5)
        {
            var ret = new FpMatch();
            ret.matched = false;
            ret.ID = 0;
            ret.MatchScore = 0;

            DateTime to = DateTime.Now.AddSeconds(timeout);
            while (DateTime.Now < to)
            {
                if (GetImage())
                {
                    if (Image2Tz())
                    {
                        int id, score;
                        if (FastSearch(out id, out score))
                        {
                            ret.matched = true;
                            ret.ID = id;
                            ret.MatchScore = score;
                            return Task.FromResult(ret);
                        }
                    }
                }
            }
            return Task.FromResult(new FpMatch());
        }

        /// <summary>
        /// Genirates an image from the sensor.
        /// </summary>
        /// <returns>false if no finger is on the sensor</returns>
        private bool GetImage()
        {
            try
            {
                SendCommand(FingerprintCommand.GETIMAGE, new byte[0]);
                var reply = GetReply();
                if (reply != null &&
                    reply.Pid == PacketIdentifier.ACKPACKET &&
                    reply.ConfCode == ConfirmationCode.ExeComplete)
                {
                    return true;
                }
            }
            catch { }

            _wiating = false;
            return false;
        }

        /// <summary>
        /// Generate character file from the original finger image in ImageBuffer and
        /// store the file in CharBuffer1 or CharBuffer2.
        /// </summary>
        /// <param name="slot">1 or 2 any value higher than 2 will be procesed as 2</param>
        /// <returns>false if no finger is on the sensor</returns>
        private bool Image2Tz(byte slot = 1)
        {
            try
            {
                SendCommand(FingerprintCommand.IMAGE2TZ, new byte[] { slot });
                var reply = GetReply();
                if (reply != null &&
                    reply.Pid == PacketIdentifier.ACKPACKET &&
                    reply.ConfCode == ConfirmationCode.ExeComplete)
                {
                    return true;
                }
            }
            catch { }

            _wiating = false;
            return false;
        }

        /// <summary>
        /// Search the whole finger library for the template that matches the one in slot 1
        /// </summary>
        /// <param name="id"></param>
        /// <param name="matchScore"></param>
        /// <returns></returns>
        private bool FastSearch(out int id, out int matchScore)
        {
            id = 0;
            matchScore = 0;
            try
            {
                SendCommand(FingerprintCommand.HISPEEDSEARCH, new byte[] { 0x01, 0x00, 0x00, 0x03, 0xE8 });
                var reply = GetReply();
                if (reply != null &&
                    reply.Pid == PacketIdentifier.ACKPACKET &&
                    reply.ConfCode == ConfirmationCode.ExeComplete)
                {
                    id = (reply.Data[0] << 8) | (reply.Data[1] & 0xFF);
                    matchScore = (reply.Data[2] << 8) | (reply.Data[3] & 0xFF);
                    return true;
                }
            }
            catch { }

            _wiating = false;
            return false;
        }

        /// <summary>
        /// Ask the sensor to take two print feature template and create a model
        /// </summary>
        /// <returns></returns>
        private bool CreateModel()
        {
            try
            {
                SendCommand(FingerprintCommand.REGMODEL, new byte[0]);
                var reply = GetReply();
                if (reply != null &&
                    reply.Pid == PacketIdentifier.ACKPACKET &&
                    reply.ConfCode == ConfirmationCode.ExeComplete)
                {
                    return true;
                }
            }
            catch { }

            _wiating = false;
            return false;
        }

        /// <summary>
        /// Ask the sensor to store the calculated model for later matching
        /// </summary>
        /// <param name="id">The model location</param>
        /// <returns></returns>
        private bool StoreModel(int id)
        {
            try
            {
                SendCommand(FingerprintCommand.STORE, new byte[] { 0x01, (byte)(id >> 8), (byte)(id & 0xff) });
                var reply = GetReply();
                if (reply != null &&
                    reply.Pid == PacketIdentifier.ACKPACKET &&
                    reply.ConfCode == ConfirmationCode.ExeComplete)
                {
                    return true;
                }
            }
            catch { }

            _wiating = false;
            return false;
        }


        /// <summary>
        /// Send a command to the module
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        private void SendCommand(FingerprintCommand cmd, byte[] data)
        {
            FingerPrintProtocol fp = new FingerPrintProtocol(cmd, data);
            _newPacket = false;
            _wiating = true;
            _inBuff.Clear();
            _sps.Write(fp.GetStructuredPacket());
        }

        /// <summary>
        /// Get a reply from the module
        /// </summary>
        /// <returns></returns>
        private FingerPrintProtocol GetReply()
        {
            var to = DateTime.Now.AddMilliseconds(1000);
            while (to > DateTime.Now)
            {
                if (_newPacket)
                {
                    _wiating = false;
                    var fpReply = FingerPrintProtocol.Parse(_inBuff.ToArray());
                    return fpReply;
                }
                Thread.Sleep(1);
            }
            Console.WriteLine("Reply Timeout");
            _wiating = false;
            return null;
        }

        /// <summary>
        /// Static function to get a list of available serial ports
        /// </summary>
        /// <returns></returns>
        public static string[] GetSerialPorts()
        {
            return SerialDevice.GetPortNames();
        }

        public void Dispose()
        {
            _sps?.Close();
            _sps.Dispose();           
        }
    }
}
