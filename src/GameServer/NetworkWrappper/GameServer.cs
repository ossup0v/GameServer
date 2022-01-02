using GameServer.Configs;
using GameServer.Metagame.GameRooms;
using GameServer.NetworkWrappper.NetworkProcessors;

namespace GameServer.NetworkWrappper
{
    public class GameServer : IGameServer
    {
        public int MaxPlayerAmount { get; private set; }
        public int MaxGameRoomAmount { get; private set; }
        public int ClientsPort { get; private set; }
        public int GameRoomsPort { get; private set; }

        private readonly IServerSendToClient _serverSendToClient;
        private readonly IServerSendToGameRoom _serverSendToGameRoom;
        private readonly IRoomManager _roomManager;
        private readonly IClientDataReceiver _clientsDataReceiver;
        private readonly IGameRoomDataReceiver _gameRoomDataReceiver;

        public GameServer(GameServerConfig config,
            IServerSendToClient serverSendToClient,
            IServerSendToGameRoom serverSendToGameRoom,
            IRoomManager roomManager,
            IClientDataReceiver clientDataReceiver,
            IGameRoomDataReceiver gameRoomDataReceiver)
        {
            Console.WriteLine("Game server ctor was called");
            MaxPlayerAmount = config.MaxPlayerAmount;
            MaxGameRoomAmount = config.MaxGameRoomAmount;

            ClientsPort = config.ClientPort;
            GameRoomsPort = config.GameRoomPort;

            _serverSendToClient = serverSendToClient;
            _serverSendToGameRoom = serverSendToGameRoom;
            _roomManager = roomManager;
            _clientsDataReceiver = clientDataReceiver;
            _gameRoomDataReceiver = gameRoomDataReceiver;
        }

        public void Start()
        {
            Console.WriteLine("Starting server...");

            _clientsDataReceiver.StartReceive(ClientsPort, MaxPlayerAmount);
            _clientsDataReceiver.NewNetworkClientAdded += OnNewClientAdded;

            _gameRoomDataReceiver.StartReceive(GameRoomsPort, MaxGameRoomAmount);
            _gameRoomDataReceiver.NewNetworkClientAdded += OnNewGameRoomAdded;
            Console.WriteLine($"Server started on port {ClientsPort}.");
        }

        public void Stop()
        {
            _clientsDataReceiver.StopReceive();
        }

        private void OnNewClientAdded(Guid newClientId)
        {
            _serverSendToClient.Welcome(newClientId, "Welcome to Server!");
            SendUserData(newClientId);
        }

        private void OnNewGameRoomAdded(Guid newGameRoomId)
        {
            _serverSendToGameRoom.GameRoomData(newGameRoomId);
        }

        private void SendUserData(Guid toClient)
        {
            _serverSendToClient.RoomList(toClient, _roomManager.Rooms);
        }
    }
}