using GameServer.Common;
using GameServer.Network;
using System.Net.Sockets;

namespace GameServer.Metagame.GameRooms
{
    public class GameRoom
    {
        private readonly IServiceProvider _serviceProvider;

        public Guid Id { get; }
        public GameRoomData Data { get; set; }
        public NetworkClient Client { get; set; }
        public bool IsAvailableToJoin { get; set; } = true;

        public GameRoom(Guid id, IServiceProvider serviceProvider)
        {
            Id = id;
            _serviceProvider = serviceProvider;

            Client = new NetworkClient(id, serviceProvider);
        }

        public void SubsctibeToReceivePackets(Action<Guid, int, Packet> callback)
        {
            Client.tcp.PacketReceived += callback;
            Client.udp.PacketReceived += callback;
        }

        public void Connect(TcpClient tcpClient)
        {
            Client.tcp.Connect(tcpClient, Client);
        }

        public void Launch(List<MetagameUser> usersToAdd)
        {
            //launch new process here
        }

        public void RoomStarted()
        {
            //connect here all users
        }

        public Task<ApiResult> Join(MetagameUser user)
        {
            Data.Users.Add(user);

            return Task.FromResult(ApiResult.Ok);
        }

        public void Stop()
        {

        } 
    }
}
