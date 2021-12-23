using GameServer.Metagame.Room;
using GameServer.Network;

namespace GameServer.Metagame
{
    public class GameManager
    {
        public static GameManager Instance = new GameManager();
        private GameManager() { }

        public void JoinGameRoom(User user)
        {
            var room = RoomManager.Instance.CreateOrGetRoom(user);
            Thread.Sleep((int)TimeSpan.FromSeconds(2).TotalMilliseconds);
            ServerSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
        }
    }
}
