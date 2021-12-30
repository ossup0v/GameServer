namespace GameServer.Metagame.GameRoom
{
    public interface IRoomManager
    {
        GameRoom GetRoom(Guid roomId);
        GameRoom CreateRoom(User creator, string mode, string title, int maxPlayerCount);
        IReadOnlyDictionary<Guid, GameRoom> Rooms { get; }
    }
}