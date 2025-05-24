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

public class NetworkManager : INetEventListener
{
    public static NetworkManager Instance { get; } = new NetworkManager();
    private NetworkManager() { }
    private readonly Matchmaker matchmaker = new Matchmaker();
    private readonly Dictionary<NetPeer, Player> connectedPlayers = new Dictionary<NetPeer, Player>();
    private readonly Dictionary<Player, GameSession> playerToSession = new();
    private NetManager server;
    private const string VALID_KEY = "hsdbpc";

    public void Start()
    {
        server = new NetManager(this);
        server.Start(7777);
        Console.WriteLine("Server started");

        while (true)
        {
            server.PollEvents();
            Thread.Sleep(15); // around 66FPS
        }
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        string key = request.Data.GetString();
        if (key == VALID_KEY)
        {
            request.Accept();
        }
        else
        {
            request.Reject();
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        var player = new Player(peer);
        connectedPlayers[peer] = player;
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod method)
    {
    
        // byte[] data = reader.RawData[reader.Position..reader.AvailableBytes];
        // var bitReader = new BitReader(data);
        // PacketType flag;
        // flag = (PacketType)bitReader.ReadBits(3);
        int start = reader.Position;
        int length = reader.AvailableBytes;

        if (length <= 0 || start + length > reader.RawData.Length)
        {
            Console.WriteLine("[Error] Invalid slice range: Position=" + start + ", Available=" + length);
            return;
        }

        byte[] data = new byte[length];
        Array.Copy(reader.RawData, start, data, 0, length);

        var bitReader = new BitReader(data);
        PacketType flag = (PacketType)bitReader.ReadBits(3);
//        Console.WriteLine("Received: flag: " + flag + " / start = " + start + " / length = " + length);
        
        switch (flag)
        {
            case PacketType.LetsStart:
                reader.GetByte();
                matchmaker.AddPlayer(connectedPlayers[peer]
                ,reader.GetUShort(),
                reader.GetString());
                MessageSender.SendPlayerInfo(connectedPlayers[peer]);

                break;
            case PacketType.PlayerInput:
                var sender = connectedPlayers[peer];
                if (playerToSession.TryGetValue(sender, out var session))
                {
                    //byte[] payload = reader.RawData[..reader.AvailableBytes];
                    int inputData = bitReader.ReadBits(3);
                    session.ReceiveInput(sender, inputData);
                }

                break;

            case PacketType.ClientIsReady:
                var player = connectedPlayers[peer];
                playerToSession[player].StartGame(player);

                break;

            default:
            
                break;
        }
        reader.Recycle();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (connectedPlayers.Remove(peer, out var player))
        {
            Console.WriteLine($"[Server] Player disconnected: {player.ClientId}");
        }
        Console.WriteLine("Disconnected");    
    }
    
    public void RegisterSession(GameSession session)
    {
        foreach (var player in session.GetPlayers())
        {
            playerToSession[player] = session;
        }

        session.OnPlayerInputReceived += HandlePlayerInput;
        session.OnMatchFound += HandleMatchFound;
        session.OnGameEnded += HandleGameStart;
        session.OnGameEnded += HandleGameEnded;
    }
    private void HandleGameStart(List<Player> players)
    {
        MessageSender.SendGameStart(players);
    }
    private void HandlePlayerInput(Player fromPlayer, int inputData, GameSession session)
    {
        foreach (var player in session.GetPlayers())
        {
            // if (player != fromPlayer)
            // {
            //     MessageSender.SendInputPacket(player, inputData);
            // }
            MessageSender.SendInputPacket(player, inputData);
            //Console.WriteLine("send to "+player.ClientId+" / Send inputData: " + inputData);    
        }
    }
    private void HandleMatchFound(List<Player> players)
    {
        MessageSender.SendMatchFound(players);
    }

    private void HandleGameEnded(List<Player> players)
    {
        //MessageSender.SendGameEnd(players);
    }


    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) => Console.WriteLine("Error");
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
}
