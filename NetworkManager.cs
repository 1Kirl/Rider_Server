using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Net.Sockets; // SocketError
using System.Threading; // Thread.Sleep()
using Shared.Network;
using Shared.Protocol;
using Shared.Bits;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Numerics;

public class NetworkManager : INetEventListener
{
    public static NetworkManager Instance { get; } = new NetworkManager();
    private NetworkManager() { }
    private readonly Matchmaker matchmaker = new Matchmaker();
    private readonly Dictionary<NetPeer, Player> connectedPlayers = new Dictionary<NetPeer, Player>();
    private readonly Dictionary<Player, GameSession> playerToSession = new();
    private NetManager server;
    private const string VALID_KEY = "hsdbpc";

    public void Start() {
        server = new NetManager(this);
        server.Start(7777);
        Console.WriteLine("Server started");

        while (true) {
            server.PollEvents();
            Thread.Sleep(15); // roughly 66 FPS
        }
    }

    public void OnConnectionRequest(ConnectionRequest request) {
        string key = request.Data.GetString();
        if (key == VALID_KEY) {
            request.Accept();
        }
        else {
            request.Reject();
        }
    }

    public void OnPeerConnected(NetPeer peer) {
        var player = new Player(peer);
        connectedPlayers[peer] = player;
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod method) {
        int start = reader.Position;
        int length = reader.AvailableBytes;

        if (length <= 0 || start + length > reader.RawData.Length) {
            Console.WriteLine("[Error] Invalid slice range: Position=" + start + ", Available=" + length);
            return;
        }

        byte[] data = new byte[length];
        Array.Copy(reader.RawData, start, data, 0, length);

        var bitReader = new BitReader(data);
        PacketType flag = (PacketType)bitReader.ReadBits(4);

        switch (flag) {
            case PacketType.LetsStart:
                Console.WriteLine("[NM] Received: LetsStart");
                reader.GetByte(); // dump padding
                matchmaker.AddPlayer(connectedPlayers[peer], reader.GetUShort(), reader.GetString());
                break;

            case PacketType.PlayerInput:
                //Console.WriteLine("[NM] Received: PlayerInput");
                var sender = connectedPlayers[peer];
                if (playerToSession.TryGetValue(sender, out var session)) {
                    int inputData = bitReader.ReadBits(3);
                    session.ReceiveInput(sender, inputData);
                }
                break;

            case PacketType.ClientIsReady:
                Console.WriteLine("[NM] Received: ClientIsReady");
                playerToSession[connectedPlayers[peer]].StartGame(connectedPlayers[peer]);
                break;

            case PacketType.ScoreUpdate:
                var scoringPlayer = connectedPlayers[peer];
                ushort score = (ushort)bitReader.ReadBits(16); // update score from client
                scoringPlayer.CurrentScore = score;

                UpdateRankings(scoringPlayer);
                break;

            case PacketType.StopFinding:
                Console.WriteLine("[NM] Received: StopFinding");
                matchmaker.RemovePlayer(connectedPlayers[peer]);
                break;

            case PacketType.ReachedFinishLine: {
                    Console.WriteLine("[NM] Received: FinishFlag");
                    reader.GetByte(); // dump padding
                    var player = connectedPlayers[peer];
                    ushort finishscore = reader.GetUShort();
                    player.CurrentScore = finishscore;
                    player.HasReachedFinish = true;

                    if (playerToSession.TryGetValue(player, out var finishsession)) {
                        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        long relativeFinishTime = now - finishsession.GetStartTime();
                        player.FinishTimestamp = relativeFinishTime;

                        Console.WriteLine($"[Server] Player {player.ClientId} reached finish line at +{relativeFinishTime}ms");

                        // 1��: ���� ������ ���� Ÿ�̸� ����
                        finishsession.StartEarlyEndTimerIfNotRunning();
                    }
                    break;
                }

            case PacketType.TransformUpdate:
                //Console.WriteLine("[NM] Received: PositionUpdate");
                reader.GetByte(); // dump padding
                var position_sender = connectedPlayers[peer];
                Vector3 pos = new Vector3
                (
                    reader.GetFloat(), // x
                    reader.GetFloat(), // y
                    reader.GetFloat()  // z
                );

                Quaternion rot = new Quaternion(
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat()
                );
                if (playerToSession.TryGetValue(position_sender, out var position_session))
                {
                    position_session.ReceiveTransform(position_sender, pos, rot);
                }
                break;
            case PacketType.Effect:
                Console.WriteLine("[NM] Received: Effect");
                var effect_sender = connectedPlayers[peer];
                if (playerToSession.TryGetValue(effect_sender, out var effect_session)) {
                    int effectData = bitReader.ReadBits(3);
                    Console.WriteLine($"[NM] Effect: {effectData}");
                    effect_session.ReceiveEffect(effect_sender, effectData);
                }
                break;
            default:
                break;
        }
        reader.Recycle();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
        if (connectedPlayers.Remove(peer, out var player)) {
            Console.WriteLine($"[Server] Player disconnected: {player.ClientId}");
        }
        Console.WriteLine("Disconnected");
    }

    public void RegisterSession(GameSession session) {
        foreach (var player in session.GetPlayers()) {
            playerToSession[player] = session;
        }

        session.OnPlayerInputReceived += HandlePlayerInput;
        session.OnPlayerEffectReceived += HandlePlayerEffect;
        session.OnPlayerTransformReceived += HandlePlayerTransform;
        session.OnMatchFound += HandleMatchFound;
        session.OnGameStart += HandleGameStart;
        session.OnGameEnded += HandleGameEnded;
    }

    public void SendMyInfo(Player player) {
        MessageSender.SendPlayerInfo(player);
    }

    public void WaitingMember(List<Player> waitingPlayers) {
        MessageSender.SendWaitingPlayers(waitingPlayers);
    }

    private void HandleGameStart(List<Player> players, long startTime) {
        MessageSender.SendGameStart(players, startTime);
    }

    private void HandlePlayerInput(Player fromPlayer, int inputData, GameSession session) {
        foreach (var player in session.GetPlayers())
        {
            if (player != fromPlayer)
            {
                MessageSender.SendInputPacket(player, fromPlayer, inputData);
            }
        }
    }
    private void HandlePlayerEffect(Player fromPlayer, int effectData, GameSession session) {
        foreach (var player in session.GetPlayers())
        {
            if (player != fromPlayer)
            {
                MessageSender.SendEffectPacket(player, fromPlayer, effectData);
            }
        }
    }
    private void HandlePlayerTransform(Player fromPlayer, Vector3 pos, Quaternion rot, GameSession session) {
        foreach (var player in session.GetPlayers())
        {
            if (player != fromPlayer)
            {
                MessageSender.SendTransformPacket(player, fromPlayer, pos, rot);
            }
        }
    }

    private void HandleMatchFound(List<Player> players) {
        MessageSender.SendMatchFound(players);
    }

    private void HandleGameEnded(List<Player> players) {
        MessageSender.SendGameEnd(players);

    }

    private void UpdateRankings(Player fromPlayer) {
        var sortedPlayers = playerToSession[fromPlayer].GetPlayers()
            .OrderByDescending(p => p.CurrentScore)
            .ToList();
        MessageSender.SendRankings(sortedPlayers);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) => Console.WriteLine("Error");
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
}
