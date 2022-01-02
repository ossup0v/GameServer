﻿namespace GameServer.Metagame.GameRooms
{
    public class GameRoomData
    {
        public Guid RoomId { get; set; }
        public int Port { get; set; }
        public int MaxPlayerCount { get; set; }
        public string Mode { get; set; }
        public string Title { get; set; }
        public MetagameUser Creator { get; set; }
        public List<MetagameUser> Users { get; set; } = new List<MetagameUser>();
    }
}