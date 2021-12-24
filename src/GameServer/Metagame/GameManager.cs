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
            ServerSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
        }

        public void CreateRoom(User user)
        {
            var room = RoomManager.Instance.CreateOrGetRoom(user);
#warning не, нужно пофиксить это, а то полное г......вно...
            Thread.Sleep((int)TimeSpan.FromSeconds(0.25).TotalMilliseconds);
            ServerSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
        }
    }
}
