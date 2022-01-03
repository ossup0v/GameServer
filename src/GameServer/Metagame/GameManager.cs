using GameServer.Common;
using GameServer.Metagame.GameRooms;
using GameServer.NetworkWrappper;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.Metagame
{
    public class GameManager : IGameManager
    {
        private readonly IServerSendToClient _serverSend;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoomManager _roomManager;
        private readonly ILogger<GameManager> _log;

        public GameManager(IServerSendToClient serverSend, 
            IServiceProvider serviceProvider,
            IRoomManager roomManager,
            ILogger<GameManager> log)
        {
            _serverSend = serverSend;
            _serviceProvider = serviceProvider;
            _roomManager = roomManager;
            _log = log;
        }

        public Task<ApiResult> JoinGameRoom(Guid roomId, MetagameUser user)
        {
            _log.ZLogInformation("User trying to join a game room");
            var room = _roomManager.GetRoom(roomId);
            if (room == null)
            {
                _log.ZLogError($"User {user?.Data?.Id}-{user?.Data?.Username} can't find room with id {roomId}");
                return Task.FromResult(ApiResult.Error("Can't create room"));
            }
            room.Join(user);
            _serverSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
            _log.ZLogInformation($"Sended port {room.Data.Port} to user");

            return Task.FromResult(ApiResult.Ok);
        }

        public Task<ApiResult> CreateRoom(MetagameUser user, string mode, string title, int maxPlayerCount)
        {
            return Task.FromResult(_roomManager.CreateRoom(user, mode, title, maxPlayerCount));
        }
    }
}
