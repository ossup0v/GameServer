using GameServer.Metagame;
using GameServer.Metagame.GameRooms;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.NetworkWrappper
{
    public class ServerGameRoomPacketsHandler : IHostedService
    {
        private readonly IClientHolder _clientHolder;
        private readonly IGameRoomHolder _gameRoomHolder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerSendToClient _serverSend;
        private readonly IGameRoomDataReceiver _gameRoomDataReceiver;
        private readonly IGameManager _gameManager;
        private readonly ILogger<ServerGameRoomPacketsHandler> _log;

        public delegate Task PacketHandler(Guid fromClient, Packet packet);
        private Dictionary<int, PacketHandler> _handlers;

        private void InitializeHandlers()
        {
            _handlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ToServerFromGameRoom.gameRoomLaunched, GameRoomLaunched }
            };

            _log.ZLogInformation("Game room initialized packets.");
        }

        public ServerGameRoomPacketsHandler(IClientHolder clientHolder,
            IGameRoomHolder gameRoomHolder,
            IServiceProvider serviceProvider,
            IServerSendToClient serverSend,
            IGameRoomDataReceiver  gameRoomDataReceiver,
            IGameManager gameManager,
            ILogger<ServerGameRoomPacketsHandler> log)
        {
            _clientHolder = clientHolder;
            _gameRoomHolder = gameRoomHolder;
            _serviceProvider = serviceProvider;
            _serverSend = serverSend;
            _gameRoomDataReceiver = gameRoomDataReceiver;
            _gameManager = gameManager;
            _log = log;
            InitializeHandlers();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _gameRoomDataReceiver.PacketReceived += OnPacketReceived;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _gameRoomDataReceiver.PacketReceived -= OnPacketReceived;

            return Task.CompletedTask;
        }

        private void OnPacketReceived(Guid fromGameRoom, int packetId, Packet packet)
        {
            if (_handlers.TryGetValue(packetId, out var packetHandler))
            {
                packetHandler(fromGameRoom, packet);
            }
            else
            {
                _log.ZLogError($"Error! can't process packer with packet id {packetId}, from game room {fromGameRoom}");
            }
        }

        private Task GameRoomLaunched(Guid fromGameRoom, Packet packet)
        {
            _log.ZLogInformation("Game room laucnhed !");

            var clientIdCheck = packet.ReadGuid();

            _log.ZLogInformation($"Welcome received from id on server {fromGameRoom}, in packet {clientIdCheck}");
            _log.ZLogInformation($"{_gameRoomHolder.Get(fromGameRoom)?.Client.tcp.Socket.Client.RemoteEndPoint} connected successfully and is now game room {fromGameRoom}.");

            if (fromGameRoom != clientIdCheck)
            {
                _log.ZLogError($"Player (ID: {fromGameRoom}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            var creatorId = packet.ReadGuid();
            var mode = packet.ReadString();
            var title = packet.ReadString();
            var maxPlayerCount = packet.ReadInt();
            var gameRoomPort = packet.ReadInt();

            var creatorOfGameRoom = _clientHolder.Get(creatorId)?.MetagameUser;

            if (creatorOfGameRoom == null)
            {
                _log.ZLogError($"Error! can't find creator of game room! Mode:{mode} Title:{title} MaxPlayers:{maxPlayerCount} RoomId:{fromGameRoom}");
            }

            _gameRoomHolder.Get(fromGameRoom).Data = new GameRoomData
            {
                Title = title,
                Mode = mode,
                MaxPlayerCount = maxPlayerCount,
                RoomId = fromGameRoom,
                Creator = creatorOfGameRoom,
                Port = gameRoomPort,
                Users = new List<MetagameUser>()
            };

            if (creatorOfGameRoom != null)
                _gameManager.JoinGameRoom(fromGameRoom, creatorOfGameRoom);

            return Task.CompletedTask;
        }
    }
}
