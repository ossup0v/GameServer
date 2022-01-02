using GameServer.Metagame.GameRooms;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using System.Net;
using System.Net.Sockets;

namespace GameServer.NetworkWrappper.NetworkProcessors
{
    public class GameRoomNetworkProcessor : NetworkProcessorBase<IGameRoomHolder, Guid, GameRoom>, IGameRoomDataSender, IGameRoomDataReceiver
    {
        public GameRoomNetworkProcessor(IGameRoomHolder gameRoomHolder, IServiceProvider serviceProvider) : base(gameRoomHolder, serviceProvider) { }
        
        public Action<Guid> NewNetworkClientAdded { get; set; } = delegate { };
        
        public void SendDataTCP(Guid toRoom, Packet packet)
        {
            Holder.Get(toRoom)?.Client.tcp.SendData(packet);
        }

        public void SendDataUDP(Guid toRoom, Packet packet)
        {
            SendUDPData(Holder.Get(toRoom)?.Client.udp?.EndPoint, packet);
        }

        protected override void TCPConnectCallback(IAsyncResult result)
        {
            var client = default(TcpClient);
            try
            {
                client = TcpListener.EndAcceptTcpClient(result);
                TcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            }
            catch (Exception ex) { }

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            if (IsAvailableToConnect)
            {
                ConnectNewGameRoom(client);
                return;
            }
        }
        
        private void ConnectNewGameRoom(TcpClient client)
        {
            var newGameRoomId = Guid.NewGuid();
            var newGameRoom = new GameRoom(newGameRoomId, ServiceProvider);

            newGameRoom.Connect(client);
            newGameRoom.SubsctibeToReceivePackets(PacketReceived);

            Holder.AddNew(newGameRoom);

            NewNetworkClientAdded(newGameRoomId);
        }

        protected override void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = UdpListener.EndReceive(result, ref clientEndPoint);
                UdpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                ProcessUdpData(data, clientEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        private void ProcessUdpData(byte[] data, IPEndPoint clientEndPoint)
        {
            using (Packet packet = new Packet(data))
            {
                var gameRoomId = packet.ReadGuid();

                if (gameRoomId == default(Guid))
                {
                    return;
                }

                if (Holder.Get(gameRoomId)?.Client.udp.EndPoint == null)
                {
                    Holder.Get(gameRoomId)?.Client.udp.Connect(clientEndPoint);
                    return;
                }

                if (Holder.Get(gameRoomId)?.Client.udp?.EndPoint?.ToString() == clientEndPoint.ToString())
                {
                    Holder.Get(gameRoomId)?.Client.udp.HandleData(packet);
                }
            }
        }
    }
 
    public interface IGameRoomDataSender : IDataSender<Guid> { }

    public interface IGameRoomDataReceiver : IDataReceiver<Guid> { }
}
