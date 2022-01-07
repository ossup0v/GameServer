using GameServer.Common;
using GameServer.Metagame.GameRooms.MetagameRooms;

namespace GameServer.Metagame.GameRooms
{
    public interface IRoomManager
    {
        ApiResult<MetagameGameRoom> GetFirstAvailableToJoin();
        ApiResult<GameRoom> GetRoom(Guid roomId);
        IEnumerable<GameRoom> Rooms { get; }
        ApiResult JoinGameRoom(MetagameUser user);
        ApiResult LeaveGameRoom(MetagameUser user);
        ApiResult GameRoomSessionEnd(int port);
    }
}