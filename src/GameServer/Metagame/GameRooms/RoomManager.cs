using GameServer.Common;
using GameServer.Configs;
using GameServer.Metagame.GameRooms.MetagameRooms;
using GameServer.NetworkWrappper;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ZLogger;

namespace GameServer.Metagame.GameRooms
{
    public class RoomManager : IRoomManager
    {
        private readonly List<int> _availablePorts; //new List<int>() { 26952, 26953, 26955, 26956, 26957, 26958, 26959, 26960 }
        public IEnumerable<GameRoom> Rooms => _gameRoomHolder.GetAll();
        private readonly GameServerConfig _gameServerConfig;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGameRoomHolder _gameRoomHolder;
        private readonly IMetagameRoomHolder _metagameRoomHolder;
        private readonly ILogger<RoomManager> _log;

        public RoomManager(
            RoomManagerConfig roomManagerConfig,
            GameServerConfig gameServerConfig,
            IServiceProvider serviceProvider,
            IGameRoomHolder gameRoomHolder,
            IMetagameRoomHolder metagameRoomHolder,
            ILogger<RoomManager> log)
        {
            _availablePorts = roomManagerConfig.AvailablePorts;
            _gameServerConfig = gameServerConfig;
            _serviceProvider = serviceProvider;
            _gameRoomHolder = gameRoomHolder;
            _metagameRoomHolder = metagameRoomHolder;
            _log = log;
        }

        public ApiResult<GameRoom> GetRoom(Guid roomId)
        {
            return ApiResult<GameRoom>.OK(_gameRoomHolder.Get(roomId));
        }

        public ApiResult<GameRoom> GetFirstAvailableToJoin()
        {
            var existsRoom = _gameRoomHolder.GetAll().FirstOrDefault(x => x.IsAvailableToJoin)
                ?? CreateRoom().Value;

            if (existsRoom != null)
                return ApiResult<GameRoom>.OK(existsRoom);

            return ApiResult<GameRoom>.Error("Can't crate room for join");

        }

        public ApiResult CreateRoom(MetagameUser creator, string mode, string title, int maxPlayerCount)
        {
            var roomId = Guid.NewGuid();

            var availablePort = _availablePorts.FirstOrDefault();

            if (availablePort == 0)
            {
                _log.ZLogError("All ports is using! can't create room!");
                return ApiResult.Failed("All ports is using! can't create room!");
            }

            _availablePorts.Remove(availablePort);

            Process.Start(Constants.RoomExePath, GetGameRoomParams(
                availablePort,
                roomId,
                mode,
                title,
                maxPlayerCount,
                creator.Id));

            _log.ZLogInformation($"Room lauched on {availablePort} port!");
            return ApiResult.Ok;
        }

        private ApiResult<GameRoom> CreateRoom()
        {
            var roomId = Guid.NewGuid();

            var availablePort = _availablePorts.FirstOrDefault();

            if (availablePort == 0)
            {
                _log.ZLogError("All ports is using! can't create room!");
                return ApiResult<GameRoom>.Failed("All ports is using! can't create room!");
            }

            _availablePorts.Remove(availablePort);

            const string Mode = "Created by server mode";
            const string Title = "Created by server";

            var newGameRoom = new GameRoom(roomId, _serviceProvider)
            {
                Data = new GameRoomData()
                {
                    RoomId = roomId,
                    MaxPlayerCount = 12,
                    Port = availablePort,
                    Title = Title,
                    Mode = Mode
                }
            };

            _metagameRoomHolder.AddNew(new MetagameGameRoom(roomId, availablePort, _serviceProvider)
            {
                Users = newGameRoom.Data.Users
            });

            return ApiResult<GameRoom>.OK(newGameRoom);
        }

        private string GetGameRoomParams(int roomPort, Guid roomId, string mode, string title, int maxPlayerCount, Guid creatorId)
        {
            return $"{roomPort};{_gameServerConfig.GameRoomPort};{roomId};{mode};{title};{maxPlayerCount};{creatorId}";
        }

        public ApiResult JoinGameRoom(MetagameUser user)
        {
            var result = GetFirstAvailableToJoin();

            if (!result.Success)
                return result;

            result.Value.Join(user);

            return ApiResult.Ok;
        }

        public ApiResult LeaveGameRoom(MetagameUser user)
        {
            var rooms = _gameRoomHolder.GetAll().Where(x => x.Data.Users.ContainsKey(user.Id));

            if (rooms == null || !rooms.Any())
            {
                _log.ZLogError($"Can't find user game {user}");
                return ApiResult.Failed($"Can't find user game {user}");
            }

            if (rooms.Count() > 1)
            { 
                _log.ZLogError($"User {user} joined in multiple game. game ids {string.Join(" ", rooms.Select(x =>x.Id))}");
            }

            foreach (var room in rooms)
            {
                room.Leave(user);
            }

            return ApiResult.Ok;
        }
    }
}
