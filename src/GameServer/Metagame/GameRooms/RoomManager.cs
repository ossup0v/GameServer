using GameServer.Common;
using GameServer.Configs;
using GameServer.NetworkWrappper.Holders;
using System.Diagnostics;

namespace GameServer.Metagame.GameRooms
{
    public class RoomManager : IRoomManager
    {
        private readonly List<int> _availablePorts; //new List<int>() { 26952, 26953, 26955, 26956, 26957, 26958, 26959, 26960 }
        public IEnumerable<GameRoom> Rooms => _gameRoomHolder.GetAll();
        private readonly GameServerConfig _gameServerConfig;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGameRoomHolder _gameRoomHolder;

        public RoomManager(
            RoomManagerConfig roomManagerConfig, 
            GameServerConfig gameServerConfig, 
            IServiceProvider serviceProvider,
            IGameRoomHolder gameRoomHolder)
        {
            _availablePorts = roomManagerConfig.AvailablePorts;
            _gameServerConfig = gameServerConfig;
            _serviceProvider = serviceProvider;
            _gameRoomHolder = gameRoomHolder;
        }

        public GameRoom? GetRoom(Guid roomId)
        {
            return _gameRoomHolder.Get(roomId);
        }

        public GameRoom? GetFirstAvailableToJoin()
        {
            return _gameRoomHolder.GetAll().FirstOrDefault(x => x?.IsAvailableToJoin ?? false);
        }

        public ApiResult CreateRoom(MetagameUser creator, string mode, string title, int maxPlayerCount)
        {
            var roomId = Guid.NewGuid();

            var availablePort = _availablePorts.FirstOrDefault();

            if (availablePort == 0)
            {
                Console.WriteLine("All ports is using! can't create room!");
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
            
            Console.WriteLine($"Room lauched on {availablePort} port!");
            return ApiResult.Ok;
        }

        private string GetGameRoomParams(int roomPort, Guid roomId, string mode, string title, int maxPlayerCount, Guid creatorId)
        {
            return $"{roomPort};{_gameServerConfig.GameRoomPort};{roomId};{mode};{title};{maxPlayerCount};{creatorId}";
        }
    }
}
