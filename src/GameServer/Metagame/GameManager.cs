using GameServer.Metagame.Room;
using GameServer.Network;

namespace GameServer.Metagame
{
    public class GameManager
    {
        public static GameManager Instance = new GameManager();
        private GameManager() { }

        public void JoinGameRoom(Guid roomId, User user)
        {
            Console.WriteLine("User trying to join a game room");
            var room = RoomManager.Instance.GetRoom(roomId);
            if (room == null)
            {
                Console.WriteLine($"User {user?.Data?.Id}-{user?.Data?.Username} can't find room with id {roomId}");
                return;
            }
            room.Join(user);
            ServerSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
            Console.WriteLine($"Sended port {room.Data.Port} to user");
        }

        public void CreateRoom(User user, string mode, string title, string maxPlayerCount)
        {
            var room = RoomManager.Instance.CreateRoom(user, mode, title, maxPlayerCount);
            
            if (room == null)
            {
                return;
            }

            room.Join(user);
#warning не, нужно пофиксить это, а то полное г......вно...
            Thread.Sleep((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
            ServerSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
        }
    }
}
