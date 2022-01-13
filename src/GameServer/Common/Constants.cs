namespace GameServer.Common
{
    public static class Constants
    {
#warning fix it, use config!
#if DEBUG
        public static string RoomUrlToConntect = "127.0.0.1";
        public static string RoomExePath = @"C:\.dev\unity\UnityGameRoom\Builds\Windows\Server\Server.exe";
#else
        public static string RoomUrlToConntect = "3.66.29.169";
        public static string RoomExePath = @"Server/Server.x86_64";
#endif

        public static int CountOfPlayersToStartGameRoom = 4;

        public static int TeamCount = 4;
    }
}
