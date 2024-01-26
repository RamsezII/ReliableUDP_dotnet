using System.Text;

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

        public static ushort Remaining(this Stream stream) => (ushort)(stream.Length - stream.Position);

        public static void BeginWrite(this BinaryWriter writer, out ushort prefixePos)
        {
            prefixePos = (ushort)writer.BaseStream.Position;
            writer.Write((ushort)0);
        }
        public static void EndWrite(this BinaryWriter writer, in ushort prefixePos)
        {
            ushort length = (ushort)(writer.BaseStream.Position - prefixePos - sizeof(ushort));
            writer.BaseStream.Position = prefixePos;
            writer.Write(length);
            writer.BaseStream.Position += length;
        }

        public static void WriteRudpHeader(this BinaryWriter writer, in RudpHeader header) => header.Write(writer);
        public static RudpHeader ReadRudpHeader(this BinaryReader reader) => new(reader);
    }
}