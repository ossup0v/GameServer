namespace GameServer.Metagame.GameRooms
{
    public class GameRoomResult
    {
        public Dictionary<int, TeamScore> TeamResult = new Dictionary<int, TeamScore>();
    }

    public class TeamScore
    {
        public int Team { get; set; }
        public int Plase { get; set; }
        public Guid[] PlayerIds { get; set; } = Array.Empty<Guid>();
        public int KilledMobs { get; set; }
        public int KilledPlayers { get; set; }
        public int DeadPlayers { get; set; }

        public override string? ToString()
        {
            return $"{nameof(Team)} {Team} " +
                $"{nameof(Plase)} {Plase} " +
                $"{nameof(PlayerIds)} {string.Join(";", PlayerIds)} " +
                $"{nameof(KilledMobs)} {KilledMobs} " +
                $"{nameof(KilledPlayers)} {KilledPlayers} " +
                $"{nameof(DeadPlayers)} {DeadPlayers}";
        }
    }
}
