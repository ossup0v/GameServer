using GameServer.Common;
using GameServer.Configs;
using System.Diagnostics;

namespace GameServer.Metagame.GameRoom
{
    public class RoomManager : IRoomManager
    {
        private List<int> availablePorts = new List<int>() { 26952, 26953, 26955, 26956, 26957, 26958, 26959, 26960 };
        public IReadOnlyDictionary<Guid, GameRoom> Rooms => _rooms;
        private Dictionary<Guid, GameRoom> _rooms = new Dictionary<Guid, GameRoom>();

        public RoomManager(RoomManagerConfig roomManagerConfig)
        {
            availablePorts = roomManagerConfig.AvailablePorts;
        }

        public GameRoom GetRoom(Guid guid)
        {
            return _rooms[guid];
        }

        public GameRoom? GetFirstAvailableToJoin()
        {
            return _rooms.Values.FirstOrDefault(x => x?.IsAvailableToJoin ?? false);
        }

        public GameRoom CreateRoom(User creator, string mode, string title, int maxPlayerCount)
        {
            var roomId = Guid.NewGuid();

            var availablePort = availablePorts.FirstOrDefault();

            if (availablePort == 0)
            {
                Console.WriteLine("All ports is using! can't create room!");
                return null;
            }
            
            availablePorts.Remove(availablePort);

            Process.Start(Constants.RoomExePath, availablePort.ToString());
            Console.WriteLine($"Room lauched on {availablePort} port!");

            var newRoom = new GameRoom(new RoomData 
            { 
                RoomId = roomId ,
                Creator = creator, 
                Mode = mode, 
                Title = title,
                MaxPlayerCount = maxPlayerCount,
                Port = availablePort, 
            });

            _rooms.Add(roomId, newRoom);

            return newRoom;
        }
    }
}
