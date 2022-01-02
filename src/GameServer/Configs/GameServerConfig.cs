namespace GameServer.Configs
{
    public class GameServerConfig
    {
        public const string SectionName = "GameServerConfig";

        public int ClientPort;

        public int GameRoomPort;

        public int MaxPlayerAmount;

        public int MaxGameRoomAmount;
    }
}
