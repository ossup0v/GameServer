using GameServer.Common;
using GameServer.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace GameServer.Metagame.GameRooms
{
    public class GameRoom : IWithId<Guid>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameRoom> _log;

        public GameRoom(Guid id, IServiceProvider serviceProvider)
        {
            Id = id;
            Data.RoomId = id;
            _serviceProvider = serviceProvider;

            _log = _serviceProvider.GetRequiredService<ILogger<GameRoom>>();

            Client = new NetworkClient(id);
        }

        public Guid Id { get; }
        public GameRoomData Data { get; set; } = new GameRoomData();
        public NetworkClient Client { get; set; }

        public void SubsctibeToReceivePackets(Action<Guid, int, Packet> callback)
        {
            Client.tcp.PacketReceived += callback;
            Client.udp.PacketReceived += callback;
        }

        public void Connect(TcpClient tcpClient)
        {
            Client.tcp.Connect(tcpClient, Client);
        }

        public Task<ApiResult> Leave(MetagameUser user)
        {
            Data.Users.Remove(user.Id);
            return Task.FromResult(ApiResult.Ok);
        }
    }
}
