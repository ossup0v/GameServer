using GameServer.Common;
using GameServer.Metagame.GameRooms;
using GameServer.NetworkWrappper;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.Metagame
{
    public class GameManager : IGameManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoomManager _roomManager;
        private readonly ILogger<GameManager> _log;

        public GameManager(IServiceProvider serviceProvider,
            IRoomManager roomManager,
            ILogger<GameManager> log)
        {
            _serviceProvider = serviceProvider;
            _roomManager = roomManager;
            _log = log;
        }

        public Task<ApiResult> GameRoomSessionEnd(int port)
        {
            _log.ZLogInformation($"Port {port} is available now");
         
            return Task.FromResult(_roomManager.GameRoomSessionEnd(port));
        }

        public Task<ApiResult> JoinGameRoom(Guid roomId, MetagameUser user)
        {
            _log.ZLogInformation("User trying to join a game room");

            return Task.FromResult(_roomManager.JoinGameRoom(user));
        }
        
        public Task<ApiResult> LeaveGameRoom(MetagameUser user)
        {
            _log.ZLogInformation("User trying to leave a game room");

            return Task.FromResult(_roomManager.LeaveGameRoom(user));
        }

        //public Task<ApiResult> CreateRoom(MetagameUser user, string mode, string title, int maxPlayerCount)
        //{
        //    return Task.FromResult(_roomManager.CreateRoom(user, mode, title, maxPlayerCount));
        //}
    }
}
