using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace _RUDP_
{
    public static partial class Util
    {
        public static void WriteBool(this BinaryWriter writer, in bool value) => writer.Write((byte)(value ? 1 : 0));
        public static bool ReadBool(this BinaryReader reader) => reader.ReadByte() != 0;

        public static void WriteStr(this BinaryWriter writer, in string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            writer.Write((ushort)buffer.Length);
            writer.Write(buffer);
        }
        public static string ReadStr(this BinaryReader reader)
        {
            ushort length = reader.ReadUInt16();
            byte[] buffer = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(buffer);
        }

        public static void WriteIPEnd(this BinaryWriter writer, in IPEndPoint value)
        {
            writer.Write((uint)value.Address.Address);
            writer.Write((ushort)value.Port);
        }

        public static IPEndPoint ReadIPEndPoint(this BinaryReader reader)
        {
            uint address = reader.ReadUInt32();
            ushort port = reader.ReadUInt16();
            return new IPEndPoint(address, port);
        }

        public static bool HasNext(this Stream stream) => stream.Position < stream.Length;
        public static ushort Remaining(this Stream stream) => (ushort)(stream.Length - stream.Position);

        public static void BeginWrite(this BinaryWriter writer, out ushort prefixePos)
        {
            prefixePos = (ushort)writer.BaseStream.Position;
            writer.Write((ushort)0);
        }

        public static void EndWriteRUDP(this BinaryWriter writer, in ushort prefixePos) => EndWrite(writer, prefixePos, RudpSocket.DATA_SIZE);
        public static void EndWrite(this BinaryWriter writer, in ushort prefixePos, in ushort limitError)
        {
            ushort length = (ushort)(writer.BaseStream.Position - prefixePos - sizeof(ushort));
            writer.BaseStream.Position = prefixePos;
            writer.Write(length);
            writer.BaseStream.Position += length;
            if (length > limitError)
                Debug.LogError($"Paquet size is too big: {length} > {limitError}");
        }

        public static ushort PredictDataSize_unprefixed(this BinaryReader reader, in ushort max_size)
        {
            ushort initialPos = (ushort)reader.BaseStream.Position;
            ushort dataLength = 0;

            while (reader.BaseStream.HasNext())
            {
                ushort length = reader.ReadUInt16();
                if (dataLength + length <= max_size)
                {
                    dataLength += length;
                    reader.BaseStream.Position += length;
                }
                else
                    break;
            }

            reader.BaseStream.Position = initialPos;
            return dataLength;
        }

        public static void CopyData_unprefixed(this BinaryReader reader, in byte[] destBuf, ushort start)
        {
            while (reader.BaseStream.Remaining() > 0)
            {
                ushort length = reader.ReadUInt16();
                if (start + length > destBuf.Length)
                {
                    reader.BaseStream.Position -= sizeof(ushort);
                    break;
                }
                else
                {
                    reader.Read(destBuf, start, length);
                    start += length;
                }
            }
        }
    }
}