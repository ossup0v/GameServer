using GameServer.Common;

namespace GameServer.Metagame
{
    public interface IGameManager
    {
        Task<ApiResult> GameRoomSessionEnd(int port);
        Task<ApiResult> JoinGameRoom(Guid roomId, MetagameUser user);
        Task<ApiResult> LeaveGameRoom(MetagameUser user);
        //Task<ApiResult> CreateRoom(MetagameUser user, string mode, string title, int maxPlayerCount);
    }
}