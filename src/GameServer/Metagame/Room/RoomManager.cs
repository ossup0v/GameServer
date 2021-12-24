using GameServer.Common;
using GameServer.Network;
using System.Diagnostics;

namespace GameServer.Metagame.Room
{
    public class RoomManager
    {
        public IReadOnlyDictionary<Guid, Room> Rooms => _rooms;
        private Dictionary<Guid, Room> _rooms = new Dictionary<Guid, Room>();
        public static RoomManager Instance = new RoomManager();
        private RoomManager() 
        {
            CreateRoom(new User { Data = new UserData { Username = "test1" } });
            CreateRoom(new User { Data = new UserData { Username = "test2" } });
        }

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

            return CreateRoom(creator);
        }

        public Room CreateRoom(User creator)
        {
            var roomId = Guid.NewGuid();

            var availablePort = Server.FreeTcpPort();

            Process.Start(Constants.RoomExePath, availablePort.ToString());
            var newRoom = new Room(new RoomData { Creator = creator, Title = "test room", Port = availablePort, Mode = "poka ne sdelal", RoomId = roomId });
            _rooms.Add(roomId, newRoom);

            return newRoom;
        }
    }
}
