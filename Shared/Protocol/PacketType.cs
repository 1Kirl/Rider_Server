namespace Shared.Protocol
{
    // we use 3bits for the packet type
    public enum PacketType : byte
    {
        LetsStart = 0,
        MyInfo = 1, // 1 bit
        GameStart = 2,
        PlayerInput = 3, // 2 bits
        MatchFound = 4,
        ClientIsReady = 5,
        RankingsUpdate = 6,
        ScoreUpdate = 7, // 3 bits
        WaitingMember = 8,
        StopFinding = 9,
        GameEnd = 10,
        ServerResultSummary = 11,
        ReachedFinishLine = 12,
        CountdownStart = 13
    }
}