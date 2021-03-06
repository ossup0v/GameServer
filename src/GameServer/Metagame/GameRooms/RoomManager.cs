using GameServer.Common;
using GameServer.Configs;
using GameServer.Metagame.GameRooms.MetagameRooms;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.Metagame.GameRooms
{
    public class RoomManager : IRoomManager
    {
        private readonly List<int> _availablePorts;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMetagameRoomHolder _metagameRoomHolder;
        private readonly ILogger<RoomManager> _log;

        public RoomManager(
            RoomManagerConfig roomManagerConfig,
            IServiceProvider serviceProvider,
            IMetagameRoomHolder metagameRoomHolder,
            ILogger<RoomManager> log)
        {
            _availablePorts = roomManagerConfig.AvailablePorts;
            _serviceProvider = serviceProvider;
            _metagameRoomHolder = metagameRoomHolder;
            _log = log;
        }

        public ApiResult<MetagameGameRoom> GetFirstAvailableToJoin()
        {
            var existsRoom = _metagameRoomHolder.GetAll().FirstOrDefault(x => x.IsAvailableToJoin)
                ?? CreateRoom().Value;

            if (existsRoom != null)
                return ApiResult<MetagameGameRoom>.OK(existsRoom);

            return ApiResult<MetagameGameRoom>.Error("Can't crate room for join");
        }

        private ApiResult<MetagameGameRoom> CreateRoom()
        {
            var roomId = Guid.NewGuid();

            var availablePort = _availablePorts.FirstOrDefault();

            if (availablePort == 0)
            {
                _log.ZLogError("All ports is using! can't create room!");
                return ApiResult<MetagameGameRoom>.Failed("All ports is using! can't create room!");
            }

            _availablePorts.Remove(availablePort);

            var newMetagameRoom = new MetagameGameRoom(
                roomId,
                availablePort,
                _serviceProvider);

            _metagameRoomHolder.AddNew(newMetagameRoom);

            return ApiResult<MetagameGameRoom>.OK(newMetagameRoom);
        }

        public ApiResult JoinGameRoom(MetagameUser user)
        {
            var result = GetFirstAvailableToJoin();

            if (!result.Success)
                return result;

            result.Value.JoinAndTryLaunchGameRoom(user);

            return ApiResult.Ok;
        }

        public ApiResult LeaveGameRoom(MetagameUser user)
        {
            var rooms = _metagameRoomHolder.GetAll().Where(x => x.Users.ContainsKey(user.Id));

            if (rooms == null || !rooms.Any())
            {
                _log.ZLogError($"Can't find user game {user}");
                return ApiResult.Failed($"Can't find user game {user}");
            }

            if (rooms.Count() > 1)
            {
                _log.ZLogError($"User {user} joined in multiple game. game ids {string.Join(" ", rooms.Select(x => x.Id))}");
            }

            foreach (var room in rooms)
            {
                room.Leave(user);
            }

            return ApiResult.Ok;
        }

        public ApiResult GameRoomSessionEnd(int port)
        {
            if (_availablePorts.Contains(port))
                _log.ZLogError($"Port {port} is already available, all available port {string.Join(" ", _availablePorts)}");
            else
                _availablePorts.Add(port);

            return ApiResult.Ok;
        }
    }
}
