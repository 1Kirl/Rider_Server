using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Protocol;

namespace Shared.Network
{
    public static class PacketParser
    {
        public static void Parse(NetPacketReader reader, out PacketType flag)
        {
            flag = (PacketType)reader.GetByte();     // 1 byte
        }

        public static void ParseWithPayload<T>(NetPacketReader reader, out PacketType flag, out ushort clientId, out T payload)
            where T : INetSerializable, new()
        {
            flag = (PacketType)reader.GetByte();
            clientId = reader.GetUShort();

            payload = new T();
            payload.Deserialize(reader);
        }
    }
}
