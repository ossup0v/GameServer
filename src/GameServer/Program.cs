
using GameServer.Network;

namespace GameServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Server.Start(150, 26954);
            Console.ReadLine();
        }
    }
}