using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    public class NetworkProcessor : IDataSender, IDataReceiver
    {
        private readonly IClientHolder _clientHolder;
        private readonly IServiceProvider _serviceProvider;
        private TcpListener _tcpListener;
        private UdpClient _udpListener;
        private int _maxPlayers;

        public Action<Guid> NewClientAdded { get; set; } = delegate { };

        public NetworkProcessor(IClientHolder clientHolder, 
            IServiceProvider serviceProvider)
        {
            _clientHolder = clientHolder;
            _serviceProvider = serviceProvider;
        }
        
        public void SendDataTCP(Guid toClient, Packet packet)
        {
            _clientHolder.GetClient(toClient)?.tcp.SendData(packet);
        }

        public void SendDataUDP(Guid toClient, Packet packet)
        {
            SendUDPData(_clientHolder.GetClient(toClient)?.udp?.EndPoint, packet);
        }

        public void StopReceive()
        { 
            _tcpListener.Stop();
            _udpListener.Close();
        }

        public void StartReceive(int port, int maxPlayers)
        {
            _maxPlayers = maxPlayers;

            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            _udpListener = new UdpClient(port);
            _udpListener.BeginReceive(UDPReceiveCallback, null);
        }

        private void TCPConnectCallback(IAsyncResult result)
        {
            var client = default(TcpClient);
            try
            {
                client = _tcpListener.EndAcceptTcpClient(result);
                _tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            }
            catch (Exception ex) { }

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            if (_clientHolder.GetAllClients().Count() < _maxPlayers)
            {
                var newGuid = Guid.NewGuid();
                var newClient = new Client(newGuid, _serviceProvider);

                newClient.tcp.Connect(client, newClient);
                newClient.tcp.PacketReceived += OnPacketReceived;
                newClient.udp.PacketReceived += OnPacketReceived;
                _clientHolder.AddNewClient(newClient);

                NewClientAdded(newGuid);
                return;
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(_result, ref clientEndPoint);
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

                    if (_clientHolder.GetClient(clientId)?.udp.EndPoint == null)
                    {
                        _clientHolder.GetClient(clientId)?.udp.Connect(clientEndPoint);
                        return;
                    }

                    if (_clientHolder.GetClient(clientId)?.udp?.EndPoint?.ToString() == clientEndPoint.ToString())
                    {
                        _clientHolder.GetClient(clientId)?.udp.HandleData(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        private void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
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

        private void OnPacketReceived(Guid fromClient, int packetId, Packet packet)
        {
            PacketReceived(fromClient, packetId, packet);
        }

        public Action<Guid, int, Packet> PacketReceived { get; set; } = delegate { };
    }

    public interface IDataSender
    { 
        void SendDataUDP(Guid toClient, Packet packet);
        void SendDataTCP(Guid toClient, Packet packet);
    }

    public interface IDataReceiver
    {
        void StartReceive(int port, int maxPlayers);
        void StopReceive();

        Action<Guid> NewClientAdded { get; set; }
        Action<Guid, int, Packet> PacketReceived { get; set; }
    }
}
