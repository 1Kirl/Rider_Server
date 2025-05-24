using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using Shared.Protocol;
using Shared.Bits;
using System.Dynamic;

namespace Shared.Network
{
    public static class MessageSender
    {
        // server says "here are all the information of players! including you"
        public static void SendMatchFound(List<Player> players)
        {
            foreach (var player in players)
            {
                foreach (var otherPlayer in players)
                {
                    var packetMaking = new BitWriter();
                    packetMaking.WriteBits((int)PacketType.MatchFound, 3);
                    packetMaking.WriteBits((int)players.Count, 3);
                    byte[] packet = packetMaking.ToArray();

                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(packet);
                    writer.Put(otherPlayer.ClientId);
                    writer.Put(otherPlayer.CarKind);
                    writer.Put(otherPlayer.Name);
                    player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                    Console.WriteLine($"[Sender] Send PlayerInfo of a room id:{otherPlayer.ClientId} / name: {otherPlayer.Name}");

                }
            }
        }
        // server says "it's your info now!"
        public static void SendPlayerInfo(Player player)
        {
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.MyInfo, 3);
            packetMaking.WriteBits((int)player.ClientId & 0b111, 3);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Sender] send single player info: {(int)player.ClientId}");
        }
        public static void SendGameStart(List<Player> players)
        {
            long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 2000;
            foreach (var player in players)
            {
                var packetMaking = new BitWriter();
                packetMaking.WriteBits((int)PacketType.GameStart, 3);
                byte[] packet = packetMaking.ToArray();

                NetDataWriter writer = new NetDataWriter();
                writer.Put(packet);
                writer.Put(startTime);
                player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                Console.WriteLine($"[Sender] Send startTime to player if:{player.ClientId}");

            }
        }

        public static void SendInputPacket(Player player, int inputData)
        {
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.PlayerInput, 3);
            packetMaking.WriteBits((int)player.ClientId & 0b111, 3);
            packetMaking.WriteBits((int)inputData & 0b111, 3);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            player.Peer.Send(writer, DeliveryMethod.Unreliable);
        }
        /*
        public static void SendMatchFound(Player player)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)PacketType.MatchFound);
            player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        public static void SendGameEnd(IEnumerable<Player> players)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)PacketType.GameEnd);

            foreach (var player in players)
                player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void SendPositionSync(Player player, float x, float y, float rotation)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)PacketType.PositionSync);
            writer.Put(x);
            writer.Put(y);
            writer.Put(rotation);
            player.Peer.Send(writer, DeliveryMethod.Unreliable);
        }

        public static void SendPong(Player player)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)PacketType.Pong);
            player.Peer.Send(writer, DeliveryMethod.Unreliable);
        }
        */
        // 필요 시 다른 Send 메서드 계속 추가
    }
}
