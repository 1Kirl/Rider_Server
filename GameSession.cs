using LiteNetLib;
using LiteNetLib.Utils; // NetDataWriter
using System;
using System.Collections.Generic;
using Shared.Network;     // MessageSender
using Shared.Protocol;    // PacketType

public class GameSession
{
    public int SessionId { get; }
    public event Action<List<Player>>? OnMatchFound;
    public event Action<List<Player>>? OnGameEnded;
    public event Action<List<Player>>? OnGameStart;
    public event Action<Player, int, GameSession>? OnPlayerInputReceived;

    private List<Player> players;
    private int readyPlayer = 0;
    public GameSession(int sessionId, List<Player> players)
    {
        this.SessionId = sessionId;
        this.players = players;
    }

    public void MatchFound()
    {
        Console.WriteLine($"[GameSession {SessionId}] Match Found.");
        OnMatchFound?.Invoke(players);
    }
    public void StartGame(Player player)
    {
        readyPlayer++;
        if (readyPlayer >= players.Count)
        {
            readyPlayer = 0;
            OnGameStart?.Invoke(players);
        }
    }
    public void EndGame()
    {
        Console.WriteLine($"[GameSession {SessionId}] Game ended.");
        OnGameEnded?.Invoke(players);
    }
    public void ReceiveInput(Player fromPlayer, int inputData)
    {
        // 서버는 물리 연산 없이 입력만 중계할 경우
        OnPlayerInputReceived?.Invoke(fromPlayer, inputData, this);
    }
    public bool HasPlayer(Player player)
    {
        return players.Contains(player);
    }

    public List<Player> GetPlayers()
    {
        return players;
    }
}

