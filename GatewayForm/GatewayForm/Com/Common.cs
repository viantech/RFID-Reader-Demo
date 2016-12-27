using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatewayForm
{
    public class Common
    {
        public static event SocketReceivedHandler Log_Msg;
        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct FrameFormat
        {
            public byte command;
            public UInt16 length;
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public byte[] metal_data;
            public byte checksum;
        };

        public struct SubFrameFormat
        {
            public byte header;
            public UInt16 length;
            public byte[] metal_data;
            public UInt16 truncate;
            public byte checksum;
        };
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        struct TcpKeepAlive
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            //[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 12)]
            private unsafe fixed byte Bytes[12];

            [System.Runtime.InteropServices.FieldOffset(0)]
            public uint On_Off;

            [System.Runtime.InteropServices.FieldOffset(4)]
            public uint KeepaLiveTime;

            [System.Runtime.InteropServices.FieldOffset(8)]
            public uint KeepaLiveInterval;
            public byte[] ToArray()
            {
                unsafe
                {
                    fixed (byte* ptr = Bytes)
                    {
                        IntPtr p = new IntPtr(ptr);
                        byte[] BytesArray = new byte[12];

                        System.Runtime.InteropServices.Marshal.Copy(p, BytesArray, 0, BytesArray.Length);
                        return BytesArray;
                    }
                }
            }
        }

        public static int SetKeepAliveValues(System.Net.Sockets.Socket Socket, bool On_Off, uint KeepaLiveTime, uint KeepaLiveInterval)
        {
            int Result = -1;

            unsafe
            {
                TcpKeepAlive KeepAliveValues = new TcpKeepAlive();

                KeepAliveValues.On_Off = Convert.ToUInt32(On_Off);
                KeepAliveValues.KeepaLiveTime = KeepaLiveTime;
                KeepAliveValues.KeepaLiveInterval = KeepaLiveInterval;
                
                /*byte[] InValue = new byte[12];

                for (int I = 0; I < 12; I++)
                    InValue[I] = KeepAliveValues.Bytes[I];*/

                Result = Socket.IOControl(System.Net.Sockets.IOControlCode.KeepAliveValues, KeepAliveValues.ToArray(), null);
            }

            return Result;
        }
        /*public static bool SetKeepAlive(System.Net.Sockets.Socket sock, ulong time, ulong interval)
        {
            try
            {
                // resulting structure
                byte[] SIO_KEEPALIVE_VALS = new byte[3 * 4];

                // array to hold input values
                ulong[] input = new ulong[3];

                // put input arguments in input array
                if (time == 0 || interval == 0) // enable disable keep-alive
                    input[0] = (0UL); // off
                else
                    input[0] = (1UL); // on

                input[1] = (time); // time millis
                input[2] = (interval); // interval millis

                // pack input into byte struct
                for (int i = 0; i < input.Length; i++)
                {
                    SIO_KEEPALIVE_VALS[i * 4 + 3] = (byte)(input[i] >> ((4 - 1) * 4) & 0xff);
                    SIO_KEEPALIVE_VALS[i * 4 + 2] = (byte)(input[i] >> ((4 - 2) * 4) & 0xff);
                    SIO_KEEPALIVE_VALS[i * 4 + 1] = (byte)(input[i] >> ((4 - 3) * 4) & 0xff);
                    SIO_KEEPALIVE_VALS[i * 4 + 0] = (byte)(input[i] >> ((4 - 4) * 4) & 0xff);
                }
                // create bytestruct for result (bytes pending on server socket)
                byte[] result = BitConverter.GetBytes(0);
                // write SIO_VALS to Socket IOControl
                sock.IOControl(System.Net.Sockets.IOControlCode.KeepAliveValues, SIO_KEEPALIVE_VALS, result);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }*/
        public enum COMMAND
        {
            CONNECTION_REQUEST_CMD = 0x01,
            GET_CONFIGURATION_CMD = 0x02,
            SET_CONFIGURATION_CMD = 0x03,
            GET_RFID_CONFIGURATION_CMD = 0x04,
            SET_RFID_CONFIGURATION_CMD = 0x05,
            START_OPERATION_CMD = 0x06,
            STOP_OPERATION_CMD = 0x07,
            GET_PORT_PROPERTIES_CMD = 0x08,
            SET_PORT_PROPERTIES_CMD = 0x09,
            REQUEST_TAG_ID_CMD = 0x0A,
            DIS_CONNECT_CMD = 0x0B,
            CHECK_READER_STT_CMD = 0x0C,
            SET_POWER_CMD = 0x0D,
            GET_POWER_CMD = 0x0E,
            SET_REGION_CMD = 0x0F,
            GET_REGION_CMD = 0x10,
            SET_POWER_MODE_CMD = 0x11,
            GET_POWER_MODE_CMD = 0x12,
            SET_CONN_TYPE_CMD = 0x13,
            SET_BLF_CMD = 0x14,
            GET_BLF_CMD = 0x15,
            REBOOT_CMD = 0x16,
            ANTENA_CMD = 0x17,
            SET_PLAN_CMD = 0x18,
            GET_PLAN_CMD = 0x19,
            FIRMWARE_UPDATE_CMD = 0x1B,
            SETTING_SENSOR_CMD = 0x1C,
        };

        public enum HEADER
        {
            PACKET_HDR = 0xE5,
            RESP_PACKET_HDR = 0xE4
        };

        public enum LENGTH : ushort
        {
            FRAME_NON_DATA = 0x04,
            SUB_FRAME_NON_DATA = 0x09,
            SIZE_SUB_FRAME_APPEND = 0x06,
            MAX_SIZE_TCP_META_DATA = 0x03FA, // 1024 - 6
            MAX_SIZE_ZIGBEE_META_DATA = 0x42, //72 -6
            MAX_SIZE_ZIGBEE_BLOCK = 0x66, //102
            CHUNK_SIZE_FILE = 0xFFDC, //65500
        };

        public enum TYPECONNECT
        {
            HDR_ZIGBEE = 0,
            HDR_WIFI = 1,
            HDR_BLUETOOTH = 2,
            HDR_ETHERNET = 3,
            HDR_RS232 = 4
        };

        #region API Encode Data to byte stream

        /// <summary>
        /// Calculate Checksum for byte data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte Chcksum(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
                sum += data[i];
            sum = (byte)(~sum + 1);

            return sum;
        }

        /// <summary>
        /// Encoding tcp packet to byte streaming
        /// </summary>
        /// <param name="sub_fmt"></param>
        /// <returns></returns>
        public static byte[] Encode_SubFrame(SubFrameFormat sub_fmt)
        {
            byte[] send_bytes = new byte[sub_fmt.length + 1];
            send_bytes[0] = sub_fmt.header;
            send_bytes[1] = (byte)((sub_fmt.length >> 8) & 0xff);
            send_bytes[2] = (byte)(sub_fmt.length & 0xff);
            if (sub_fmt.length > 5)
                Buffer.BlockCopy(sub_fmt.metal_data, 0, send_bytes, 3, sub_fmt.length - 5);
            send_bytes[sub_fmt.length - 2] = (byte)((sub_fmt.truncate >> 8) & 0xff);
            send_bytes[sub_fmt.length - 1] = (byte)(sub_fmt.truncate & 0xff);
            send_bytes[sub_fmt.length] = Chcksum(send_bytes.Skip(1).ToArray(), sub_fmt.length - 1);
            return send_bytes;
        }

        /// <summary>
        /// Encoding user data to byte with Frame Format
        /// </summary>
        /// <param name="fmt"></param>
        /// <returns></returns>
        public static byte[] Encode_Frame(FrameFormat fmt)
        {
            byte[] byte_frame = new byte[fmt.length];
            byte_frame[0] = fmt.command;
            byte_frame[1] = (byte)((fmt.length >> 8) & 0xff);
            byte_frame[2] = (byte)(fmt.length & 0xff);
            if (fmt.length > 4)
                Buffer.BlockCopy(fmt.metal_data, 0, byte_frame, 3, fmt.length - 4);
            byte_frame[fmt.length - 1] = Chcksum(byte_frame, fmt.length - 1);
            return byte_frame;
        }
        #endregion

        #region API Decode byte stream receive to meta data
        /// <summary>
        /// Decoding the SubFrameFormat is sent via TCP connection.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        /// <returns>Meta Data bytes</returns>
        public static byte[] Decode_SubFrame(byte[] buffer, int len)
        {
            SubFrameFormat sub_fmt = new SubFrameFormat();
            //int datalen_subfmt;
            /* Sub Frame Format*/
            // Header ignore
            if (buffer == null || buffer.Length == 0)
                return null;
            sub_fmt.header = buffer[0];
            if (buffer[0] != (byte)HEADER.RESP_PACKET_HDR)
            {
                MessageBox.Show("The TCP packet not send by RFID Gateway");
                return null;
            }
            //Length
            sub_fmt.length = (ushort)(buffer[2] + (buffer[1] << 8)); // deduce 1 byte header
            if (sub_fmt.length != len - 1)
                return null;

            //Check CRC
            sub_fmt.checksum = buffer[len - 1];
            if (sub_fmt.checksum != Chcksum(buffer.Skip(1).ToArray(), sub_fmt.length - 1))
                return null;

            /*Data of TCP packet is part of Frame Format*/
            sub_fmt.metal_data = new byte[len - 6];
            Buffer.BlockCopy(buffer, 3, sub_fmt.metal_data, 0, len - 6);
            //Number of TCP packet be slpited in meta data
            //sub_fmt.truncate = (ushort)(buffer[len - 2] + (buffer[len - 3] << 8));

            //return sub_fmt.metal_data;
            return sub_fmt.metal_data;
        }
        /// <summary>
        /// Decoding FrameFormat is slpit by RFID gateway
        /// </summary>
        /// <param name="meta_sub"></param>
        /// <returns></returns>
        public static byte[] Decode_Frame(byte command_option, byte[] meta_subframe)
        {
            FrameFormat fmt = new FrameFormat();
            //int datalen_fmt;
            if (meta_subframe == null || meta_subframe.Length == 0)
                return null;
            /* Frame Format*/
            //Command
            fmt.command = meta_subframe[0];
            if (fmt.command != command_option)
                return new byte[] { 0x01, 0x01 };

            //Length
            fmt.length = (ushort)(meta_subframe[2] + (meta_subframe[1] << 8));
            if (fmt.length != meta_subframe.Length)
                return new byte[] { 0x01, 0x01 };

            //CRC
            fmt.checksum = meta_subframe[fmt.length - 1];
            if (fmt.checksum != Chcksum(meta_subframe, fmt.length - 1))
                return new byte[] { 0x01, 0x01 };

            /* Data Frame Format*/
            fmt.metal_data = new byte[fmt.length - 4];
            Buffer.BlockCopy(meta_subframe, 3, fmt.metal_data, 0, fmt.length - 4);
            return fmt.metal_data;
        }

        public static byte Decode_Frame_ACK(byte command_option, byte[] meta_subframe)
        {
            FrameFormat fmt = new FrameFormat();
            if (meta_subframe == null || meta_subframe.Length == 0)
                return 0x01;
            /* Frame Format*/
            //Command
            fmt.command = meta_subframe[0];
            if (fmt.command != command_option)
                return 0x01;

            //Length
            fmt.length = (ushort)(meta_subframe[2] + (meta_subframe[1] << 8));
            if (fmt.length != 5)
                return 0x01;

            //CRC
            fmt.checksum = meta_subframe[4];
            if (fmt.checksum != Chcksum(meta_subframe, 4))
                return 0x01;

            return meta_subframe[3];
        }
        #endregion

        #region Get Data Command
        public static string Get_Data(byte[] byte_data)
        {
            if (byte_data != null && byte_data.Length > 0)
            {
                if (0x00 == byte_data[0])
                {
                    return Encoding.ASCII.GetString(byte_data.Skip(1).ToArray(), 0, byte_data.Length - 1);
                }
                else
                {
                    return "NAK\n";
                }
            }
            else
                return String.Empty;
        }
        #endregion

        #region Events
        
        
        public static void Log_Raise(string log_str)
        {
            SocketReceivedHandler logmsg = Log_Msg;
            if (logmsg != null)
                logmsg(log_str);
        }
        /*
        public static void Cmd_Raise(string cmd_str)
        {
            SocketReceivedHandler cmd_msg = ConfigMessage;
            if (cmd_msg != null)
                cmd_msg(cmd_str);
        }
        public static void TagID_Raise(string ID_str)
        {
            SocketReceivedHandler messageReceived = MessageReceived;
            if (messageReceived != null)
                messageReceived(ID_str);
        }*/

        
        #endregion
    }
}
