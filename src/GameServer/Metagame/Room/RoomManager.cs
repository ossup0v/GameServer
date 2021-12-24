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
            CreateRoom(new User { Data = new UserData { Username = "test1" } }, "mode", "title", "16");
            CreateRoom(new User { Data = new UserData { Username = "test2" } }, "mode", "title", "16");
        }

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

            var availablePort = Server.FreeTcpPort();

            Process.Start(Constants.RoomExePath, availablePort.ToString());

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
