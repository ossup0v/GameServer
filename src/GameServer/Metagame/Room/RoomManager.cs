using GameServer.Common;
using System.Diagnostics;

namespace GameServer.Metagame.Room
{
    public class RoomManager
    {
        private List<int> _availableProts = new List<int>() { 26950 };
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

        public Room? CreateOrGetRoom(User creator)
        {
            var existsRoom = GetFirstAvailableToJoin();
            
            if (existsRoom != null)
            {
                return existsRoom;
            }

            var roomId = Guid.NewGuid();

            var roomPort = _availableProts.FirstOrDefault();
            if (roomPort == 0)
            {
                Console.WriteLine("Can't create room! have no available ports!");
                return null;
            }

            _availableProts.Remove(roomPort);

            Process.Start(Constants.RoomExePath, roomPort.ToString());
            var newRoom = new Room(new RoomData { Creator = creator, Title = "test room", Port = roomPort });
            _rooms.Add(roomId, newRoom);

            return newRoom;
        }
    }
}
