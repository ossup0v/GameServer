using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using ZLogger;

namespace GameServer.NetworkWrappper.NetworkProcessors
{
    public class ClientNetworkProcessor : NetworkProcessorBase<IClientHolder, Guid, User>, IClientDataSender, IClientDataReceiver
    {
        public ClientNetworkProcessor(IClientHolder clientHolder, IServiceProvider serviceProvider, ILogger<ClientNetworkProcessor> log) 
            : base(clientHolder, serviceProvider, log) { }

        public Action<Guid> NewNetworkClientAdded { get; set; } = delegate { };

        public void SendDataTCP(Guid toClient, Packet packet)
        {
            Holder.Get(toClient)?.Client.tcp.SendData(packet);
        }

        public void SendDataUDP(Guid toClient, Packet packet)
        {
            SendUDPData(Holder.Get(toClient).Client.udp?.EndPoint, packet);
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
                Log.ZLogError($"Error receiving UDP data: {ex}");
            }
        }

        private void ProcessUdpData(byte[] data, IPEndPoint clientEndPoint)
        {
            using (Packet packet = new Packet(data))
            {
                var clientId = packet.ReadGuid();

                if (clientId == default(Guid))
                {
                    return;
                }

                if (Holder.Get(clientId)?.Client.udp.EndPoint == null)
                {
                    Holder.Get(clientId)?.Client.udp.Connect(clientEndPoint);
                    return;
                }

                if (Holder.Get(clientId)?.Client.udp?.EndPoint?.ToString() == clientEndPoint.ToString())
                {
                    Holder.Get(clientId)?.Client.udp.HandleData(packet);
                }
            }
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

            Log.ZLogInformation($"Incoming connection from {client?.Client?.RemoteEndPoint}...");

            if (IsAvailableToConnect)
            {
                ConnectNewUser(client);
                return;
            }

            Log.ZLogError($"{client?.Client?.RemoteEndPoint} failed to connect: Server full!");
        }

        private void ConnectNewUser(TcpClient client)
        {
            var userId = Guid.NewGuid();
            var newUser = new User(userId, ServiceProvider.GetRequiredService<ILogger<User>>(), ServiceProvider);
            
            newUser.Connect(client);
            newUser.SubsctibeToReceivePackets(PacketReceived);

            Holder.AddNew(newUser);

            NewNetworkClientAdded(userId);
        }
    }

    public interface IClientDataSender : IDataSender<Guid> { }

    public interface IClientDataReceiver : IDataReceiver<Guid> { }
}
