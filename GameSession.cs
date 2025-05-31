using LiteNetLib;
using LiteNetLib.Utils; // NetDataWriter
using System;
using System.Collections.Generic;
using Shared.Network;     // MessageSender
using Shared.Protocol;
using System.Diagnostics;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Numerics;    // PacketType

public class GameSession
{
    public int SessionId { get; }
    public event Action<List<Player>>? OnMatchFound;
    public event Action<List<Player>>? OnGameEnded;
    public event Action<List<Player>, long>? OnGameStart;
    public event Action<Player, int, GameSession>? OnPlayerInputReceived;
    public event Action<Player, Vector3, Quaternion, GameSession>? OnPlayerTransformReceived;
    private long startTime = 0;
    private long gamePlayTime = 0;
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
        Console.WriteLine($"[GameSession {SessionId}] Whole Players: {players.Count} / ReadyPlayers: {readyPlayer}");
        if (readyPlayer >= players.Count)
        {
            readyPlayer = 0;
            startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 2000;
            OnGameStart?.Invoke(players, startTime);
            this.GameTimer();
        }
    }
    private async void GameTimer()
    {
        gamePlayTime = 60000; // 60초
        var delay = startTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (delay > 0)
            await Task.Delay((int)delay); // 게임 시작까지 대기

        Console.WriteLine($"[GameSession {SessionId}] Game started!");

        await Task.Delay((int)gamePlayTime); // 게임 플레이 시간 대기

        EndGame(); // 게임 종료 처리
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
    public void ReceiveTransform(Player fromPlayer, Vector3 pos, Quaternion rot)
    {
        // 서버는 물리 연산 없이 입력만 중계할 경우
        OnPlayerTransformReceived?.Invoke(fromPlayer, pos, rot, this);
    }
    public bool HasPlayer(Player player)
    {
        return players.Contains(player);
    }

    public List<Player> GetPlayers()
    {
        return players;
    }
    public long GetStartTime() {
        return startTime;
    }
}

