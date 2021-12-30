using GameServer.Common;
using GameServer.Network;

namespace GameServer.Metagame
{
    public interface IGameManager
    {
        Task<ApiResult> JoinGameRoom(Guid roomId, User user);
        Task<ApiResult> CreateRoom(User user, string mode, string title, int maxPlayerCount);
    }
}