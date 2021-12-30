using GameServer.Configs;
using GameServer.Metagame.GameRoom;

namespace GameServer.Network
{
    public class GameServer : IGameServer
    {
        public int MaxPlayers { get; private set; }
        public int Port { get; private set; }

        private readonly IServerSend _serverSend;
        private readonly IRoomManager _roomManager;
        private readonly IDataReceiver _dataReceiver;

        public GameServer(GameServerConfig config,
            IServerSend serverSend,
            IRoomManager roomManager,
            IDataReceiver dataReceiver)
        {
            Console.WriteLine("Game server ctor was called");
            MaxPlayers = config.MaxPlayers;
            Port = config.Port;
            _serverSend = serverSend;
            _roomManager = roomManager;
            _dataReceiver = dataReceiver;
        }

        public void Start()
        {
            Console.WriteLine("Starting server...");
            _dataReceiver.StartReceive(Port, MaxPlayers);
            _dataReceiver.NewClientAdded += OnNewClientAdded;
            Console.WriteLine($"Server started on port {Port}.");
        }

        public void Stop()
        {
            _dataReceiver.StopReceive();
        }

        private void OnNewClientAdded(Guid newClientId)
        {
            _serverSend.Welcome(newClientId, "Welcome to Server!");
            SendUserData(newClientId);
        }

        private void SendUserData(Guid toClient)
        {
            _serverSend.RoomList(toClient, _roomManager.Rooms);
        }
    }
}