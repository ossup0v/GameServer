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
            var room = RoomManager.Instance.GetRoom(roomId);
            room.Join(user);
            ServerSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
        }

        public void CreateRoom(User user, string mode, string title, string maxPlayerCount)
        {
            var room = RoomManager.Instance.CreateRoom(user, mode, title, maxPlayerCount);
            room.Join(user);
#warning не, нужно пофиксить это, а то полное г......вно...
            Thread.Sleep((int)TimeSpan.FromSeconds(0.25).TotalMilliseconds);
            ServerSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
        }
    }
}
