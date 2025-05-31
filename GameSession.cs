using LiteNetLib;
using LiteNetLib.Utils; // NetDataWriter
using System;
using System.Collections.Generic;
using Shared.Network;     // MessageSender
using Shared.Protocol;
using System.Diagnostics;
using System.Net.Sockets;
using System.Xml.Serialization;    // PacketType

public class GameSession
{
    public int SessionId { get; }
    public event Action<List<Player>>? OnMatchFound;
    public event Action<List<Player>>? OnGameEnded;
    public event Action<List<Player>, long>? OnGameStart;
    public event Action<Player, int, GameSession>? OnPlayerInputReceived;
    private long startTime = 0;
    private long gamePlayTime = 0;
    private List<Player> players;
    private int readyPlayer = 0;
    private bool isEarlyEndTriggered = false;

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
    public void EndGame() {
        Console.WriteLine($"[GameSession {SessionId}] Game ended.");

        // 1. 도달 여부 분리
        var finishers = players.Where(p => p.HasReachedFinish).ToList();
        var nonFinishers = players.Where(p => !p.HasReachedFinish).ToList();

        // 2. 도달자: 도착 시간 기준 정렬
        finishers.Sort((a, b) => a.FinishTimestamp.CompareTo(b.FinishTimestamp));

        // 3. arrivalRank 부여
        for (int i = 0; i < finishers.Count; i++) {
            finishers[i].ArrivalRank = i + 1; // 1등부터
        }

        // 4. 미도달자는 점수만으로 정렬
        var sorted = finishers.Concat(nonFinishers.OrderByDescending(p => p.CurrentScore)).ToList();

        // 5. 결과 전송 (보너스 점수 계산은 클라이언트에서)
        MessageSender.SendFinalResultSummary(sorted);

        // 6. UI 종료 트리거 등
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
    public long GetStartTime() {
        return startTime;
    }

    public void StartEarlyEndTimerIfNotRunning() {
        if (isEarlyEndTriggered) return;
        isEarlyEndTriggered = true;

        // 2번: CountdownStart 패킷 전송 (서버 시간 기준)
        long countdownStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        MessageSender.SendCountdownStart(players, countdownStartTime);

        Task.Run(async () =>
        {
            await Task.Delay(10000); // 10초 대기
            EndGame();
        });
    }

}

