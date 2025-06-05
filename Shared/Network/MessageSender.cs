using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using Shared.Protocol;
using Shared.Bits;
using System.Dynamic;
using System.Numerics;


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
                    packetMaking.WriteBits((int)PacketType.MatchFound, 4);
                    packetMaking.WriteBits((int)players.Count, 4);
                    byte[] packet = packetMaking.ToArray();

                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(packet);
                    writer.Put(otherPlayer.ClientId);
                    writer.Put(otherPlayer.CarKind);
                    writer.Put(otherPlayer.DieEffect);
                    writer.Put(otherPlayer.Trail);
                    writer.Put(otherPlayer.Name);
                    player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                    Console.WriteLine($"[Sender] /matchFound/ Send memeberInfo of a room id:{otherPlayer.ClientId} / carKind:{otherPlayer.CarKind}/ Dieeffect: {otherPlayer.DieEffect}/Trail: {otherPlayer.Trail}/name: {otherPlayer.Name}");
                }
            }
        }
        public static void SendWaitingPlayers(List<Player> waitingPlayers)
        {
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.WaitingMember, 4);
            packetMaking.WriteBits((int)waitingPlayers.Count, 4);
            byte[] packet = packetMaking.ToArray();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            foreach (var player in waitingPlayers)
            {
                player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                Console.WriteLine($"[Sender] /WaitingMember/ {waitingPlayers.Count}");
            }
        }


        // server says "it's your info now!"
        public static void SendPlayerInfo(Player player)
        {
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.MyInfo, 4);
            packetMaking.WriteBits((int)player.ClientId & 0b1111, 4);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Sender] /MyInfo/ send single player info: {(int)player.ClientId}");
        }
        public static void SendGameStart(List<Player> players, long startTime)
        {

            Console.WriteLine($"[Sender] /GameStart/ StartTime will be: {startTime}");
            foreach (var player in players)
            {
                var packetMaking = new BitWriter();
                packetMaking.WriteBits((int)PacketType.GameStart, 4);
                byte[] packet = packetMaking.ToArray();

                NetDataWriter writer = new NetDataWriter();
                writer.Put(packet);
                writer.Put(startTime);
                writer.Put(players.Count);

                foreach (var p in players)
                {
                    writer.Put(p.ClientId);
                    writer.Put(p.CurrentScore);
                    writer.Put(p.Name);
                }
                player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                Console.WriteLine($"[Sender] /GameStart/ Send startTime to player id:{player.ClientId}");
            }
        }

        public static void SendInputPacket(Player player, Player fromPlayer, int inputData)
        {
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.PlayerInput, 4);
            packetMaking.WriteBits((int)fromPlayer.ClientId & 0b1111, 4);
            packetMaking.WriteBits((int)inputData & 0b111, 3);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            player.Peer.Send(writer, DeliveryMethod.Unreliable);
            //Console.WriteLine($"[Sender] /PlayerInput/ from id:{fromPlayer.ClientId}/ to id: {player.ClientId}");
        }
        public static void SendEffectPacket(Player player, Player fromPlayer, int effectData)
        {
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.Effect, 4);
            packetMaking.WriteBits((int)fromPlayer.ClientId & 0b1111, 4);
            packetMaking.WriteBits((int)effectData & 0b111, 3);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            player.Peer.Send(writer, DeliveryMethod.Unreliable);
            //Console.WriteLine($"[Sender] /PlayerInput/ from id:{fromPlayer.ClientId}/ to id: {player.ClientId}");
        }
        public static void SendTransformPacket(Player player, Player fromPlayer, Vector3 pos, Quaternion rot)
        {
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.TransformUpdate, 4);
            packetMaking.WriteBits((int)fromPlayer.ClientId & 0b1111, 4);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);

            writer.Put(pos.X);
            writer.Put(pos.Y);
            writer.Put(pos.Z);

            writer.Put(rot.X);
            writer.Put(rot.Y);
            writer.Put(rot.Z);
            writer.Put(rot.W);

            player.Peer.Send(writer, DeliveryMethod.Unreliable);
            //Console.WriteLine($"[Sender] /PlayerInput/ from id:{fromPlayer.ClientId}/ to id: {player.ClientId}");
        }

        public static void SendRankings(List<Player> sortedPlayers)
        {
            foreach (var receiver in sortedPlayers)
            {
                var packetMaking = new BitWriter();
                packetMaking.WriteBits((int)PacketType.RankingsUpdate, 4);
                byte[] packet = packetMaking.ToArray();

                var writer = new NetDataWriter();
                writer.Put(packet);
                writer.Put(sortedPlayers.Count);

                foreach (var p in sortedPlayers)
                {
                    writer.Put(p.ClientId);
                    writer.Put(p.CurrentScore);
                    writer.Put(p.Name);
                }
                receiver.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void SendGameEnd(List<Player> players)
        {
            Console.WriteLine($"[Sender] /GameEnd/ game will be ended in 3 seconds");
            foreach (var player in players)
            {
                var packetMaking = new BitWriter();
                packetMaking.WriteBits((int)PacketType.GameEnd, 4);
                byte[] packet = packetMaking.ToArray();

                NetDataWriter writer = new NetDataWriter();
                writer.Put(packet);
                player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                Console.WriteLine($"[Sender] /GameEnd/ Send GameEnd to player id:{player.ClientId}");
            }
        }

        public static void SendFinalResultSummary(List<Player> playersInOrder) {
            foreach (var player in playersInOrder) {
                var packetMaking = new BitWriter();
                packetMaking.WriteBits((int)PacketType.ServerResultSummary, 4); // 4bit
                byte[] packet = packetMaking.ToArray();

                NetDataWriter writer = new NetDataWriter();
                writer.Put(packet);
                writer.Put(playersInOrder.Count);

                for (int i = 0; i < playersInOrder.Count; i++) {
                    var p = playersInOrder[i];
                    writer.Put(p.ClientId);
                    writer.Put(p.Name);
                    writer.Put(p.CurrentScore);
                    writer.Put((byte)(p.ArrivalRank > 0 ? p.ArrivalRank : 0));
                }

                player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                Console.WriteLine($"[Sender] /FinalResultSummary/ Sent to {player.ClientId}");
            }
        }

        // 서버에서 10초 남음 알림
        public static void SendCountdownStart(List<Player> players, long serverStartTimestamp) {
            var writer = new NetDataWriter();
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.CountdownStart, 4); // new enum

            byte[] packet = packetMaking.ToArray();
            writer.Put(packet);
            writer.Put(serverStartTimestamp); 

            foreach (var player in players)
                player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("Start counting");
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
