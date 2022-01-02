using GameServer.Metagame;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;
using Microsoft.Extensions.Hosting;

namespace GameServer.NetworkWrappper
{
    public class ServerClientPacketsHandler : IHostedService
    {
        private readonly IClientHolder _clientHolder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerSendToClient _serverSend;
        private readonly IClientDataReceiver _dataReceiver;
        private readonly IGameManager _gameManager;

        public delegate Task PacketHandler(Guid fromClient, Packet packet);
        private Dictionary<int, PacketHandler> _handlers;

        private void InitializeHandlers()
        {
            _handlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ToServerFromClient.welcomeReceived, WelcomeReceived },
                { (int)ToServerFromClient.registerUser, RegisterUser },
                { (int)ToServerFromClient.joinGameRoom, JoinGameRoom},
                { (int)ToServerFromClient.loginUser, LoginUser},
                { (int)ToServerFromClient.createGameRoom, CreateGameRoom}
            };

            Console.WriteLine("Initialized packets.");
        }

        public ServerClientPacketsHandler(IClientHolder clientHolder,
            IServiceProvider serviceProvider,
            IServerSendToClient serverSend, 
            IClientDataReceiver dataReceiver,
            IGameManager gameManager)
        {
            _clientHolder = clientHolder;
            _serviceProvider = serviceProvider;
            _serverSend = serverSend;
            _dataReceiver = dataReceiver;
            _gameManager = gameManager;

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
                Console.WriteLine($"Error! can't process packer with packet id {packetId}, from client {fromClient}");
            }
        }

        private Task WelcomeReceived(Guid fromClient, Packet packet)
        {
            Console.WriteLine("Welcome received");
            var clientIdCheck = packet.ReadGuid();

            Console.WriteLine($"Welcome received from id on server {fromClient}, in packet {clientIdCheck}");
            Console.WriteLine($"{_clientHolder.Get(fromClient)?.Client.tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            _clientHolder.Get(fromClient)?.ActiveMetagameUser();

            return Task.CompletedTask;
        }

        private async Task RegisterUser(Guid fromClient, Packet packet)
        {
            var packetId = packet.ReadGuid();
            var login = packet.ReadString();
            var password = packet.ReadString();
            var username = packet.ReadString();

            Console.WriteLine($"User registered with {login}: {password}");
            var result = await _clientHolder.Get(fromClient)?.MetagameUser.Register(login, password, username, fromClient);

            _serverSend.RegisterResult(fromClient, packetId, result.Success);
        }

        private async Task LoginUser(Guid fromClient, Packet packet)
        {
            var packetId = packet.ReadGuid();
            var login = packet.ReadString();
            var password = packet.ReadString();

            Console.WriteLine($"User registered with {login}: {password}");
            var result = await _clientHolder.Get(fromClient).MetagameUser.Login(login, password, fromClient);

            _serverSend.LoginResult(fromClient, packetId, result.Success);
        }

        private Task JoinGameRoom(Guid fromClient, Packet packet)
        {
            Console.WriteLine($"user {fromClient} joined to game!");
            var roomId = packet.ReadGuid();
            _gameManager.JoinGameRoom(roomId, _clientHolder.Get(fromClient).MetagameUser);

            return Task.CompletedTask;
        }

        private async Task CreateGameRoom(Guid fromClient, Packet packet)
        {
            Console.WriteLine($"user {fromClient} creating game room!");

            var mode = packet.ReadString();
            var title = packet.ReadString();
            var maxPlayerCount = packet.ReadInt();

            var result = await _gameManager.CreateRoom(_clientHolder.Get(fromClient).MetagameUser, mode, title, maxPlayerCount);

            if (!result.Success)
                Console.WriteLine($"Error! can't create room reson is {result.Message}");
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
