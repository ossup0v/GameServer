namespace GameServer.Metagame.GameRooms
{
    public class GameRoomData
    {
        public Guid RoomId;
        public int MaxPlayerCount;
        public string Mode;

        public int Port { get; set; }
        public string Title { get; set; }
        public MetagameUser Creator { get; set; }
        public List<MetagameUser> Users { get; set; } = new List<MetagameUser>();
    }
}
