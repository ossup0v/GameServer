using GameServer.Common;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using System.Net;
using System.Net.Sockets;

namespace GameServer.NetworkWrappper
{
    public abstract class NetworkProcessorBase<THolder, TKey, TValue> 
        where THolder : IHolder<TKey, TValue> 
        where TKey : struct 
        where TValue : class, IWithId<TKey>
    {
        protected readonly IServiceProvider ServiceProvider;
        protected TcpListener TcpListener;
        protected UdpClient UdpListener;
        protected int MaxClients;
        protected THolder Holder;

        public NetworkProcessorBase(THolder holder, IServiceProvider serviceProvider)
        {
            Holder = holder;
            ServiceProvider = serviceProvider;
        }

        public Action<Guid, int, Packet> PacketReceived { get; set; } = delegate { };

        protected bool IsAvailableToConnect => Holder.Count < MaxClients;

        public void StopReceive()
        {
            TcpListener.Stop();
            UdpListener.Close();
        }

        public void StartReceive(int port, int maxClients)
        {
            MaxClients = maxClients;

            TcpListener = new TcpListener(IPAddress.Any, port);
            TcpListener.Start();
            TcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            UdpListener = new UdpClient(port);
            UdpListener.BeginReceive(UDPReceiveCallback, null);
        }

        protected abstract void UDPReceiveCallback(IAsyncResult result);

        protected abstract void TCPConnectCallback(IAsyncResult result);

        protected void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    UdpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }
    }
}
