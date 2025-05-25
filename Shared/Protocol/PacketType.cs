namespace Shared.Protocol
{
    // we use 3bits for the packet type
    public enum PacketType : byte
    {
        LetsStart = 0,
        MyInfo = 1,
        GameStart = 2,
        PlayerInput = 3, //2 bits
        MatchFound = 4,
        ClientIsReady = 5,
        WaitingMember = 6
    }
}