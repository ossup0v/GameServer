namespace GameServer.Common
{
    public static class Constants
    {
#warning fix it, use config!
#if DEBUG
        public static string RoomExePath = @"C:\.dev\unity\UnityGameRoom\Builds\Windows\Server\Server.exe";
#else
        public static string RoomExePath = @"Server/Server.x86_64";
#endif
    }
}
