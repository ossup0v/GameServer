using GameServer.Common;
using GameServer.Configs;
using GameServer.Network;
using GameServer.NetworkWrappper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Sockets;
using ZLogger;

namespace GameServer.Metagame.GameRooms
{
    public class GameRoom : IWithId<Guid>
    {
        private readonly IServerSendToClient _serverSend;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameRoom> _log;
        private readonly GameServerConfig _gameServerConfig;
        private const int _countOfUsersToStart = 1;
        private bool _isReadyToStart => Data.Users.Count >= _countOfUsersToStart;

        public GameRoom(Guid id, IServiceProvider serviceProvider)
        {
            Id = id;
            Data.RoomId = id;
            _serviceProvider = serviceProvider;

            _gameServerConfig = _serviceProvider.GetRequiredService<GameServerConfig>();
            _serverSend = _serviceProvider.GetRequiredService<IServerSendToClient>();
            _log = _serviceProvider.GetRequiredService<ILogger<GameRoom>>();

            Client = new NetworkClient(id);
        }

        public Guid Id { get; }
        public GameRoomData Data { get; set; } = new GameRoomData();
        public NetworkClient Client { get; set; }
        public bool IsAvailableToJoin { get; private set; } = true;

        public void BanJoin()
        {
            IsAvailableToJoin = false;
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

        public Task<ApiResult> Leave(MetagameUser user)
        {
            Data.Users.Remove(user.Id);
            return Task.FromResult(ApiResult.Ok);
        }
    }
}
