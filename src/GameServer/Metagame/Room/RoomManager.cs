using GameServer.Common;
using GameServer.DAL.Mongo;
using GameServer.Network;
using System.Diagnostics;

namespace GameServer.Metagame.Room
{
    public class RoomManager
    {
        private List<int> availablePorts = new List<int>() { 26952, 26953, 26955, 26956, 26957, 26958, 26959, 26960 };
        public IReadOnlyDictionary<Guid, Room> Rooms => _rooms;
        private Dictionary<Guid, Room> _rooms = new Dictionary<Guid, Room>();
        public static RoomManager Instance = new RoomManager();

        private RoomManager() { }

        public Room GetRoom(Guid guid)
        {
            return _rooms[guid];
        }

        public Room? GetFirstAvailableToJoin()
        {
            return _rooms.Values.FirstOrDefault(x => x?.IsAvailableToJoin ?? false);
        }

        public Room CreateRoom(User creator, string mode, string title, string maxPlayerCount)
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

            if (!int.TryParse(maxPlayerCount, out var maxPlayerCountInt))
            {
                maxPlayerCountInt = 8;
            }

            var newRoom = new Room(new RoomData 
            { 
                RoomId = roomId ,
                Creator = creator, 
                Mode = mode, 
                Title = title,
                MaxPlayerCount = maxPlayerCountInt,
                Port = availablePort, 
            });

            _rooms.Add(roomId, newRoom);

            return newRoom;
        }
    }
}
