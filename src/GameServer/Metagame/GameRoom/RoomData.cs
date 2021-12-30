namespace GameServer.Metagame.GameRoom
{
    public class RoomData
    {
        public Guid RoomId;
        public int MaxPlayerCount;
        public string Mode;

        public int Port { get; set; }
        public string Title { get; set; }
        public User Creator { get; set; }
        public List<User> Users { get; set; } = new List<User>();
    }
}
