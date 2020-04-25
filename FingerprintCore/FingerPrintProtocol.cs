using System;
using System.Collections.Generic;
using System.Text;

namespace FingerprintCore
{
    public class FingerPrintProtocol
    {
        private const int OVERHEAD = 12; //< Minimum length of a valid packet
        private const int TYPE_INDEX = 6;
        private const int LENGTH_INDEX = 7;
        private const int COMAND_INDEX = 9;
        private const int DATA_INDEX = 10;
        public const UInt16 FINGERPRINT_STARTCODE = 0xEF01; //< "Wakeup" code for packet detection

        public UInt32 Address { get; set; } //< 32-bit Fingerprint sensor address
        public PacketIdentifier Pid { get; set; } //< Type of packet     
        public FingerprintCommand Command { get; set; }
        public ConfirmationCode ConfCode { get; set; }
        public byte[] Data { get; set; } //< The raw buffer for packet payload

        /// <summary>
        /// Constructor for Command packet
        /// </summary>
        /// <param name="data"></param>
        public FingerPrintProtocol(FingerprintCommand cmd, byte[] data) :
            this(PacketIdentifier.COMMANDPACKET, (byte)cmd, data)
        {

        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="data"></param>
        public FingerPrintProtocol(PacketIdentifier pid, byte cmd, byte[] data)
        {
            Pid = pid;
            Address = 0xFFFFFFFF;
            if (pid== PacketIdentifier.ACKPACKET)
            {
                Command = FingerprintCommand.None;
                ConfCode = (ConfirmationCode)cmd;
            }
            else
            {
                ConfCode = ConfirmationCode.None;
                Command = (FingerprintCommand)cmd;
            }
            Data = data;
        }

        public byte[] GetStructuredPacket()
        {
            List<byte> retVal = new List<byte>();
            retVal.Add((FINGERPRINT_STARTCODE >> 8) & 0xFF);
            retVal.Add(FINGERPRINT_STARTCODE & 0x00FF);
            retVal.Add((byte)((Address >> 24) & 0xFF));
            retVal.Add((byte)((Address >> 16) & 0xFF));
            retVal.Add((byte)((Address >> 8) & 0xFF));
            retVal.Add((byte)(Address & 0xFF));
            retVal.Add((byte)Pid);
            retVal.Add((byte)(((Data.Length + 3) >> 8) & 0xff)); // Length bytes will be set at the end
            retVal.Add((byte)((Data.Length + 3) & 0xff)); // Length bytes will be set at the end
            retVal.Add((byte)Command);
            retVal.AddRange(Data);
            retVal.AddRange(GetChecksum(retVal.ToArray()));

            return retVal.ToArray();
        }

        private static byte[] GetChecksum(byte[] pack)
        {
            byte[] csArr = new byte[2];
            UInt16 cs = 0;

            if (pack.Length > TYPE_INDEX)
            {
                for (int n = TYPE_INDEX; n < pack.Length; n++)
                {
                    cs += pack[n];
                }
                csArr[0] = (byte)((cs >> 8) & 0xff);
                csArr[1] = (byte)(cs & 0xff);
            }
            return csArr;
        }

        /// <summary>
        /// Validate a byte array as containing a valid packet.
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        /// <remarks>
        /// This assumes the first start code is the only one. If there is a rougue start code and then
        /// a complete packet after that, this validation will fail. This should be improved at some point.
        /// </remarks>
        public static bool ValidatePacket(byte[] pack)
        {
            int startIndex = 0;
            int length;
            if (pack.Length < OVERHEAD) return false;

            startIndex = FindStartIndex(pack);
            if (startIndex < 0) return false; //< There is no start sequence

            if (pack.Length < startIndex + OVERHEAD) return false;

            // Check that the packet has the correct length
            length = (pack[startIndex + LENGTH_INDEX] << 8) | (pack[startIndex + LENGTH_INDEX + 1] & 0xff);
            if (pack.Length < (startIndex + OVERHEAD + length - 3)) return false;

            // Check the checksum
            List<byte> t = new List<byte>(pack);
            t.RemoveRange(0, startIndex);
            t.RemoveRange(OVERHEAD + length - 5, t.Count - (OVERHEAD + length - 5));
            var cs = GetChecksum(t.ToArray());
            if (pack[startIndex + OVERHEAD + length - 5] != cs[0]) return false;
            if (pack[startIndex + OVERHEAD + length - 4] != cs[1]) return false;

            return true;
        }

        /// <summary>
        /// Find the start index of a valid packet
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        private static int FindStartIndex(byte[] pack)
        {
            int index = 0;
            while (index < pack.Length-2)
            {
                if (pack[index] == (FINGERPRINT_STARTCODE >> 8) &&
                    pack[index+1] == (FINGERPRINT_STARTCODE & 0xff))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Convert a data packet to a FingerPrintProtocol oject
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static FingerPrintProtocol Parse(byte[] pack)
        {
            try
            {
                List<byte> data = new List<byte>();
                int startIndex = FindStartIndex(pack);
                PacketIdentifier pid = (PacketIdentifier)pack[startIndex + TYPE_INDEX];
                byte cmd = pack[startIndex + COMAND_INDEX];

                int length = (pack[startIndex + LENGTH_INDEX] << 8) | (pack[startIndex + LENGTH_INDEX + 1] & 0xff);

                for (int n = (startIndex + DATA_INDEX); n < (startIndex + OVERHEAD + length - 5); n++)
                {
                    data.Add(pack[n]);
                }
                return new FingerPrintProtocol(pid, cmd, data.ToArray());

            }
            catch
            {
            }
            return null;
        }
    }
}
