using GameServer.Common;
using GameServer.Network;
using GameServer.NetworkWrappper;
using System.Net.Sockets;

namespace GameServer.Metagame.GameRooms
{
    public class GameRoom : IWithId<Guid>
    {
        private readonly IServiceProvider _serviceProvider;
        
        public GameRoom(Guid id, IServiceProvider serviceProvider)
        {
            Id = id;
            _serviceProvider = serviceProvider;

            Client = new NetworkClient(id);
        }

        public Guid Id { get; }
        public GameRoomData Data { get; set; }
        public NetworkClient Client { get; set; }
        public bool IsAvailableToJoin { get; set; } = true;

        public void SubsctibeToReceivePackets(Action<Guid, int, Packet> callback)
        {
            Client.tcp.PacketReceived += callback;
            Client.udp.PacketReceived += callback;
        }

        public void Connect(TcpClient tcpClient)
        {
            Client.tcp.Connect(tcpClient, Client);
        }

        public Task<ApiResult> RoomStarted()
        {
            //connect here all users
         
            return Task.FromResult(ApiResult.Ok);
        }

        public Task<ApiResult> Join(MetagameUser user)
        {
            Data.Users.Add(user);
            
            //notify others users here - user is joined

            return Task.FromResult(ApiResult.Ok);
        }

        public void Stop()
        {

        } 
    }
}
