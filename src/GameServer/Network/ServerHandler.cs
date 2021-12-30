using GameServer.Common;
using GameServer.Metagame;
using Microsoft.Extensions.Hosting;

namespace GameServer.Network
{
    public class ServerHandler : IServerHandler, IHostedService
    {
        private readonly IClientHolder _clientHolder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerSend _serverSend;
        private readonly IDataReceiver _dataReceiver;
        private readonly IGameManager _gameManager;

        public delegate Task PacketHandler(Guid fromClient, Packet packet);
        private Dictionary<int, PacketHandler> _handlers;

        private void InitializeHandlers()
        {
            _handlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientToServer.welcomeReceived, WelcomeReceived },
                { (int)ClientToServer.registerUser, RegisterUser },
                { (int)ClientToServer.joinGameRoom, JoinGameRoom},
                { (int)ClientToServer.loginUser, LoginUser},
                { (int)ClientToServer.createGameRoom, CreateGameRoom},
            };

            Console.WriteLine("Initialized packets.");
        }

        public ServerHandler(IClientHolder clientHolder,
            IServiceProvider serviceProvider,
            IServerSend serverSend, 
            IDataReceiver dataReceiver,
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

        public Task WelcomeReceived(Guid fromClient, Packet packet)
        {
            Console.WriteLine("Welcome received");
            var clientIdCheck = packet.ReadGuid();

            Console.WriteLine($"Welcome received from id on server {fromClient}, in packet {clientIdCheck}");
            Console.WriteLine($"{_clientHolder.GetClient(fromClient)?.tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            _clientHolder.GetClient(fromClient)?.CreateUser();

            return Task.CompletedTask;
        }

        public async Task RegisterUser(Guid fromClient, Packet packet)
        {
            var packetId = packet.ReadGuid();
            var login = packet.ReadString();
            var password = packet.ReadString();
            var username = packet.ReadString();

            Console.WriteLine($"User registered with {login}: {password}");
            var result = await _clientHolder.GetClient(fromClient)?.User.Register(login, password, username, fromClient);

            _serverSend.RegisterResult(fromClient, packetId, result.Success);
        }

        public async Task LoginUser(Guid fromClient, Packet packet)
        {
            var packetId = packet.ReadGuid();
            var login = packet.ReadString();
            var password = packet.ReadString();

            Console.WriteLine($"User registered with {login}: {password}");
            var result = await _clientHolder.GetClient(fromClient).User.Login(login, password, fromClient);

            _serverSend.LoginResult(fromClient, packetId, result.Success);
        }

        public Task JoinGameRoom(Guid fromClient, Packet packet)
        {
            Console.WriteLine($"user {fromClient} joined to game!");
            var roomId = packet.ReadGuid();
            _gameManager.JoinGameRoom(roomId, _clientHolder.GetClient(fromClient).User);

            return Task.CompletedTask;
        }

        public Task CreateGameRoom(Guid fromClient, Packet packet)
        {
            Console.WriteLine($"user {fromClient} creating game room!");

            var mode = packet.ReadString();
            var title = packet.ReadString();
            var maxPlayerCount = packet.ReadInt();

            _gameManager.CreateRoom(_clientHolder.GetClient(fromClient).User, mode, title, maxPlayerCount);

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
