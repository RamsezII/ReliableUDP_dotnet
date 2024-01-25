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
    }
}