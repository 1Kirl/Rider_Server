using LiteNetLib;
using System.Net;

namespace Shared.Network
{
    public class Player
    {
        public NetPeer Peer { get; }

        public ushort ClientId { get; set; }
        public ushort DieEffect { get; set; }
        public ushort Trail { get; set;}   
        public ushort CarKind { get; set; }

        public string Name { get; set; } = string.Empty;

        public Player(NetPeer peer)
        {
            Peer = peer;
        }


        public ushort CurrentScore { get; set; }
        public ushort FinalScore { get; set; }
        public bool HasReachedFinish { get; set; } = false;
        public long FinishTimestamp { get; set; } = long.MaxValue;

        //도달하지 않은 경우 -1 
        public int ArrivalRank { get; set; } = -1;
    }
}
