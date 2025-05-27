using LiteNetLib;
using System.Net;

namespace Shared.Network
{
    public class Player
    {
        /// <summary>
        /// 연결된 클라이언트를 나타내는 NetPeer 객체
        /// </summary>
        public NetPeer Peer { get; }

        /// <summary>
        /// peer.Id 기반의 int형 id
        /// </summary>
        public ushort ClientId { get; set; }
        public ushort CarKind { get; set; }
        /// <summary>
        /// 예: 닉네임, 식별용 이름 등
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public Player(NetPeer peer)
        {
            Peer = peer;
        }

        public override string ToString()
        {
            return $"[Player] ClientId={ClientId}, Name={Name}";
        }

        public int CurrentScore { get; set; }
        //public int CurrentRank { get; set; } 
    }
}
