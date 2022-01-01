using GameServer.Common;

namespace GameServer.Metagame.GameRooms
{
    public interface IRoomManager
    {
        GameRoom GetRoom(Guid roomId);
        ApiResult CreateRoom(MetagameUser creator, string mode, string title, int maxPlayerCount);
        IEnumerable<GameRoom> Rooms { get; }
    }
}