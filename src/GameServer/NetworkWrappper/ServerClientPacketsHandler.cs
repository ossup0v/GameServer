using GameServer.Metagame;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.NetworkWrappper
{
    public class ServerClientPacketsHandler : IHostedService
    {
        private readonly IClientHolder _clientHolder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerSendToClient _serverSend;
        private readonly IClientDataReceiver _dataReceiver;
        private readonly IGameManager _gameManager;
        private readonly ILogger<ServerClientPacketsHandler> _log;

        public delegate Task PacketHandler(Guid fromClient, Packet packet);
        private Dictionary<int, PacketHandler> _handlers;

        private void InitializeHandlers()
        {
            _handlers.Add((int)ToServerFromClient.welcomeReceived, WelcomeReceived);
            _handlers.Add((int)ToServerFromClient.registerUser, RegisterUser);
            _handlers.Add((int)ToServerFromClient.joinGameRoom, JoinGameRoom);
            _handlers.Add((int)ToServerFromClient.loginUser, LoginUser);
            _handlers.Add((int)ToServerFromClient.createGameRoom, CreateGameRoom);
            _handlers.Add((int)ToServerFromClient.startSearchGameRoom, StartSearchGameRoom);
            _handlers.Add((int)ToServerFromClient.cancelSearchGameRoom, CancelSearchGameRoom);

            _log.ZLogInformation("Initialized packets.");
        }

        public ServerClientPacketsHandler(IClientHolder clientHolder,
            IServiceProvider serviceProvider,
            IServerSendToClient serverSend,
            IClientDataReceiver dataReceiver,
            IGameManager gameManager,
            ILogger<ServerClientPacketsHandler> log)
        {
            _clientHolder = clientHolder;
            _serviceProvider = serviceProvider;
            _serverSend = serverSend;
            _dataReceiver = dataReceiver;
            _gameManager = gameManager;
            _log = log;

            _handlers = new Dictionary<int, PacketHandler>();
            InitializeHandlers();
        }

        private void OnPacketReceived(Guid fromClient, int packetId, Packet packet)
        {
            if (_handlers.TryGetValue(packetId, out var packetHandler))
            {
                packetHandler(fromClient, packet);
            }
            else
            {
                _log.ZLogError($"Error! can't process packer with packet id {packetId}, from client {fromClient}");
            }
        }

        private Task WelcomeReceived(Guid fromClient, Packet packet)
        {
            _log.ZLogInformation("Welcome received");
            var clientIdCheck = packet.ReadGuid();

            _log.ZLogInformation($"Welcome received from id on server {fromClient}, in packet {clientIdCheck}");
            _log.ZLogInformation($"{_clientHolder.Get(fromClient)?.Client.tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                _log.ZLogError($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            _clientHolder.Get(fromClient).JoinToServer();

            return Task.CompletedTask;
        }

        private async Task RegisterUser(Guid fromClient, Packet packet)
        {
            var packetId = packet.ReadGuid();
            var login = packet.ReadString();
            var password = packet.ReadString();
            var username = packet.ReadString();

            _log.ZLogInformation($"User registered with {login}: {password}");

            var result = await _clientHolder.Get(fromClient).MetagameUser.Register(login, password, username, fromClient);

            _serverSend.RegisterResult(fromClient, packetId, result.Success);
        }

        private async Task LoginUser(Guid fromClient, Packet packet)
        {
            var packetId = packet.ReadGuid();
            var login = packet.ReadString();
            var password = packet.ReadString();

            _log.ZLogInformation($"User registered with {login}: {password}");

            var result = await _clientHolder.Get(fromClient).MetagameUser.Login(login, password, fromClient);

            _serverSend.LoginResult(fromClient, packetId, result.Success);
        }

        private Task JoinGameRoom(Guid fromClient, Packet packet)
        {
            _log.ZLogError($"user {fromClient} trying to join room, this is obsole func, can't do this!");
            return Task.CompletedTask;
        }

        private Task CreateGameRoom(Guid fromClient, Packet packet)
        {
            _log.ZLogError($"{nameof(CreateGameRoom)} is not used now. user {fromClient} creating game room!");
            return Task.CompletedTask;
        }

        private Task StartSearchGameRoom(Guid fromClient, Packet packet)
        {
            _gameManager.JoinGameRoom(fromClient, _clientHolder.Get(fromClient).MetagameUser);
            return Task.CompletedTask;
        }

        private Task CancelSearchGameRoom(Guid fromClient, Packet packet)
        {
            _gameManager.LeaveGameRoom(_clientHolder.Get(fromClient).MetagameUser);
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _dataReceiver.PacketReceived += OnPacketReceived;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _dataReceiver.PacketReceived -= OnPacketReceived;

            return Task.CompletedTask;
        }
    }
}
