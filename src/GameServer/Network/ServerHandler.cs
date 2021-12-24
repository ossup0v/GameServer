using GameServer.Metagame;

namespace GameServer.Network
{
    public class ServerHandler
    {
        public static void WelcomeReceived(Guid fromClient, Packet packet)
        {
            Console.WriteLine("Welcome received");
            var clientIdCheck = packet.ReadGuid();

            Console.WriteLine($"Welcome received from id on server {fromClient}, in packet {clientIdCheck}");
            Console.WriteLine($"{Server.clients[fromClient].tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }
            Server.clients[fromClient].CreateUser();
            Server.clients[fromClient].SendData();
        }

        public static void RegisterUser(Guid fromClient, Packet packet)
        {
            var login = packet.ReadString();
            var password = packet.ReadString();
            var username = packet.ReadString();

            Console.WriteLine($"User registered with {login}: {password}");
            bool success = Server.clients[fromClient].User.Register(login, password, username, fromClient);

            if (!success)//retry here!
                return;
        }

        public static async void LoginUser(Guid fromClient, Packet packet)
        {
            var login = packet.ReadString();
            var password = packet.ReadString();

            Console.WriteLine($"User registered with {login}: {password}");
            bool success = await Server.clients[fromClient].User.Login(login, password, fromClient);

            if (!success)//retry here!
                return;
        }

        public static void JoinGameRoom(Guid fromClient, Packet packet)
        {
            Console.WriteLine($"user {fromClient} joined to game!");
            var roomId = packet.ReadGuid();
            GameManager.Instance.JoinGameRoom(roomId, Server.GetClient(fromClient).User);
        }
    }
}
