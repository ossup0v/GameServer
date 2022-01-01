using GameServer.Metagame.GameRooms;
using GameServer.Network.Holders;
using System.Net;
using System.Net.Sockets;

namespace GameServer.Network
{
    public interface IGameRoomDataSender
    {
        void SendDataUDP(Guid toClient, Packet packet);
        void SendDataTCP(Guid toClient, Packet packet);
    }

    public interface IGameRoomDataReceiver
    {
        void StartReceive(int port, int maxPlayers);
        void StopReceive();

        Action<Guid> NewGameRoomAdded { get; set; }
        Action<Guid, int, Packet> PacketReceived { get; set; }
    }

    public class GameRoomNetworkProcessor : NetworkProcessor, IGameRoomDataSender, IGameRoomDataReceiver
    {
        private readonly IGameRoomHolder _gameRoomHolder;
        public Action<Guid> NewGameRoomAdded { get; set; } = delegate { };

        public GameRoomNetworkProcessor(IGameRoomHolder gameRoomHolder, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _gameRoomHolder = gameRoomHolder;
        }

        protected override void TCPConnectCallback(IAsyncResult result)
        {
            var client = default(TcpClient);
            try
            {
                client = _tcpListener.EndAcceptTcpClient(result);
                _tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            }
            catch (Exception ex) { }

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            if (_gameRoomHolder.GetAll().Count() < _maxClients)
            {
                var newGameRoomId = Guid.NewGuid();
                var newGameRoom = new GameRoom(newGameRoomId, _serviceProvider);

                newGameRoom.Connect(client);
                newGameRoom.SubsctibeToReceivePackets(PacketReceived);

                _gameRoomHolder.AddNew(newGameRoom);

                NewGameRoomAdded(newGameRoomId);
                return;
            }
        }

        protected override void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);
                _udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    var gameRoomId = packet.ReadGuid();

                    if (gameRoomId == default(Guid))
                    {
                        return;
                    }

                    if (_gameRoomHolder.Get(gameRoomId)?.Client.udp.EndPoint == null)
                    {
                        _gameRoomHolder.Get(gameRoomId)?.Client.udp.Connect(clientEndPoint);
                        return;
                    }

                    if (_gameRoomHolder.Get(gameRoomId)?.Client.udp?.EndPoint?.ToString() == clientEndPoint.ToString())
                    {
                        _gameRoomHolder.Get(gameRoomId)?.Client.udp.HandleData(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        public void SendDataTCP(Guid toRoom, Packet packet)
        {
            _gameRoomHolder.Get(toRoom)?.Client.tcp.SendData(packet);
        }

        public void SendDataUDP(Guid toRoom, Packet packet)
        {
            SendUDPData(_gameRoomHolder.Get(toRoom)?.Client.udp?.EndPoint, packet);
        }
    }

    public class ClientNetworkProcessor : NetworkProcessor, IClientDataSender, IClientDataReceiver
    {
        protected readonly IClientHolder _clientHolder;
        public ClientNetworkProcessor(IClientHolder clientHolder, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _clientHolder = clientHolder;
        }

        public void SendDataTCP(Guid toClient, Packet packet)
        {
            _clientHolder.Get(toClient)?.Client.tcp.SendData(packet);
        }

        public void SendDataUDP(Guid toClient, Packet packet)
        {
            SendUDPData(_clientHolder.Get(toClient)?.Client.udp?.EndPoint, packet);
        }

        protected override void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);
                _udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    var clientId = packet.ReadGuid();

                    if (clientId == default(Guid))
                    {
                        return;
                    }

                    if (_clientHolder.Get(clientId)?.Client.udp.EndPoint == null)
                    {
                        _clientHolder.Get(clientId)?.Client.udp.Connect(clientEndPoint);
                        return;
                    }

                    if (_clientHolder.Get(clientId)?.Client.udp?.EndPoint?.ToString() == clientEndPoint.ToString())
                    {
                        _clientHolder.Get(clientId)?.Client.udp.HandleData(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        protected override void TCPConnectCallback(IAsyncResult result)
        {
            var client = default(TcpClient);
            try
            {
                client = _tcpListener.EndAcceptTcpClient(result);
                _tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            }
            catch (Exception ex) { }

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            if (_clientHolder.GetAll().Count() < _maxClients)
            {
                var userId = Guid.NewGuid();
                var newUser = new User(userId, _serviceProvider);
                newUser.Connect(client);
                newUser.SubsctibeToReceivePackets(PacketReceived);

                _clientHolder.AddNew(newUser);

                NewClientAdded(userId);
                return;
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }
     
        public Action<Guid> NewClientAdded { get; set; } = delegate { };
    }

    public abstract class NetworkProcessor
    {
        protected readonly IServiceProvider _serviceProvider;
        protected TcpListener _tcpListener;
        protected UdpClient _udpListener;
        protected int _maxClients;
        public Action<Guid, int, Packet> PacketReceived { get; set; } = delegate { };

        public NetworkProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void StopReceive()
        {
            _tcpListener.Stop();
            _udpListener.Close();
        }

        public void StartReceive(int port, int maxClients)
        {
            _maxClients = maxClients;

            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            _udpListener = new UdpClient(port);
            _udpListener.BeginReceive(UDPReceiveCallback, null);
        }

        protected abstract void UDPReceiveCallback(IAsyncResult result);

        protected abstract void TCPConnectCallback(IAsyncResult result);

        protected void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }
    }

    public interface IClientDataSender
    {
        void SendDataUDP(Guid toClient, Packet packet);
        void SendDataTCP(Guid toClient, Packet packet);
    }

    public interface IClientDataReceiver
    {
        void StartReceive(int port, int maxPlayers);
        void StopReceive();

        Action<Guid> NewClientAdded { get; set; }
        Action<Guid, int, Packet> PacketReceived { get; set; }
    }
}
