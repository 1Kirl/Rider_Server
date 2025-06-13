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
    private const int MatchSize = 8;
    private const int MatchMinSize = 2;
    private ushort nextClientId = 0;
    private bool isMatchmakingTimerRunning = false;
    private float matchmakingStartTime;
    private readonly float maxWaitTime = 20f;
    private System.Timers.Timer matchmakingTimer;
    public void AddPlayer(Player player, ushort carKind, ushort dieEffect, ushort trailId, string nickname)
    {
        ushort assingedId = nextClientId++;
        player.ClientId = assingedId;
        player.CarKind = carKind;
        player.DieEffect = dieEffect;
        player.Trail = trailId;
        player.Name = nickname;
        waitingPlayers.Add(player);
        NetworkManager.Instance.SendMyInfo(player);
        NetworkManager.Instance.WaitingMember(waitingPlayers);
        Console.WriteLine($"[Matchmaker] Player added: player #{player.ClientId}, ({waitingPlayers.Count}/{MatchSize})");

        // 최초 호출 시 타이머 시작
        if (!isMatchmakingTimerRunning)
        {
            StartMatchmakingTimer();
        }

        TryStartMatch();
    }

    private void StartMatchmakingTimer()
    {
        isMatchmakingTimerRunning = true;
        matchmakingTimer = new System.Timers.Timer(maxWaitTime * 1000);
        matchmakingTimer.Elapsed += (sender, e) =>
        {
            matchmakingTimer.Stop();
            matchmakingTimer.Dispose();
            matchmakingTimer = null;

            Console.WriteLine("[Matchmaker] Timeout reached. Forcing match start.");
            TryStartMatch(force: true);
            isMatchmakingTimerRunning = false;
        };
        matchmakingTimer.AutoReset = false;
        matchmakingTimer.Start();
    }
    private void TryStartMatch(bool force = false)
    {
        // 강제 매치 시 최소 인원 3명 이상만 있으면 매치 시작
        if (waitingPlayers.Count >= MatchSize || (force && waitingPlayers.Count >= MatchMinSize))
        {
            var matchPlayers = waitingPlayers.Take(MatchSize).ToList(); // 최대 MatchSize까지만
            waitingPlayers.RemoveRange(0, matchPlayers.Count);
            nextClientId = 0;
            int sessionId = nextSessionId++;
            var session = new GameSession(sessionId, matchPlayers);
            sessions[sessionId] = session;
            NetworkManager.Instance.RegisterSession(session);
            session.MatchFound();
            session.OnSessionDestroy += RemoveSession;

            Console.WriteLine($"[Matchmaker] GameSession {sessionId} created. / Total member: {matchPlayers.Count}");

            if (matchmakingTimer != null)
            {
                matchmakingTimer.Stop();
                matchmakingTimer.Dispose();
                matchmakingTimer = null;
            }
            isMatchmakingTimerRunning = false;
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

