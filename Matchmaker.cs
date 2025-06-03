using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Net.Sockets; // SocketError
using System.Threading; // Thread.Sleep()
using Shared.Network;
using Shared.Protocol;
public class Matchmaker
{
    private readonly List<Player> waitingPlayers = new List<Player>();
    private readonly Dictionary<int, GameSession> sessions = new Dictionary<int, GameSession>();
    private int nextSessionId = 0;
    private const int MatchSize = 2;
    private ushort nextClientId = 0;
    public void AddPlayer(Player player, ushort carKind, string nickname)
    {
        ushort assingedId = nextClientId++;
        player.ClientId = assingedId;
        player.CarKind = carKind;
        player.Name = nickname;
        waitingPlayers.Add(player);
        NetworkManager.Instance.SendMyInfo(player);
        NetworkManager.Instance.WaitingMember(waitingPlayers);
        Console.WriteLine($"[Matchmaker] Player added: player #{player.ClientId}, ({waitingPlayers.Count}/{MatchSize})");

        if (waitingPlayers.Count >= MatchSize)
        {
            var matchPlayers = waitingPlayers.Take(MatchSize).ToList();
            waitingPlayers.RemoveRange(0, MatchSize);
            nextClientId = 0;
            int sessionId = nextSessionId++;
            var session = new GameSession(sessionId, matchPlayers);
            sessions[sessionId] = session;
            NetworkManager.Instance.RegisterSession(session);
            session.MatchFound();
            session.OnSessionDestroy += RemoveSession;
            //gamestart call
            Console.WriteLine($"[Matchmaker] GameSession {sessionId} created. / Total member: {matchPlayers.Count}");
        }
    }
    public GameSession? GetSessionByPlayer(Player player)
    {
        return sessions.Values.FirstOrDefault(session => session.HasPlayer(player));
    }
    public void RemoveSession(GameSession session)
    {
        session.OnSessionDestroy -= RemoveSession;
        if(sessions.Remove(session.SessionId, out var session_removed))
        {
            Console.WriteLine($"[MatchMaker] Session {session_removed.SessionId}");
        }
    }
    public void RemovePlayer(Player player)
    {
        waitingPlayers.Remove(player);
        NetworkManager.Instance.WaitingMember(waitingPlayers); //re-send the rest of the waiting players
    }
}

