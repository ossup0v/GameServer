using GameServer.Common;

namespace GameServer.Metagame.GameRooms
{
    public interface IRoomManager
    {
        ApiResult<GameRoom> GetFirstAvailableToJoin();
        ApiResult<GameRoom> GetRoom(Guid roomId);
        ApiResult CreateRoom(MetagameUser creator, string mode, string title, int maxPlayerCount);
        IEnumerable<GameRoom> Rooms { get; }
        ApiResult JoinGameRoom(MetagameUser user);
        ApiResult LeaveGameRoom(MetagameUser user);
    }
}