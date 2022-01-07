namespace GameServer.Metagame.GameRooms
{
    public class GameRoomData
    {
        public Guid RoomId { get; set; }
        public int Port { get; set; }
        public int MaxPlayerCount { get; set; }
        public Dictionary<Guid, MetagameUser> Users { get; set; } = new Dictionary<Guid, MetagameUser>();
    }
}
