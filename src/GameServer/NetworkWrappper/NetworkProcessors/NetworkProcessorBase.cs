using GameServer.Common;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using ZLogger;

namespace GameServer.NetworkWrappper
{
    public abstract class NetworkProcessorBase<THolder, TKey, TValue> 
        where THolder : IHolder<TKey, TValue> 
        where TKey : struct
        where TValue : class, IWithId<TKey>, IWithNetworkClient
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly ILogger<NetworkProcessorBase<THolder, TKey, TValue>> Log;
        protected TcpListener TcpListener;
        protected UdpClient UdpListener;
        protected int MaxClients;
        protected THolder Holder;

        public NetworkProcessorBase(THolder holder, IServiceProvider serviceProvider, ILogger<NetworkProcessorBase<THolder, TKey, TValue>> log)
        {
            Holder = holder;
            ServiceProvider = serviceProvider;
            Log = log;
        }

        public Action<Guid> NewNetworkClientAdded { get; set; } = delegate { };
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

        public void SendDataTCP(TKey toClient, Packet packet)
        {
            Holder.Get(toClient).Client.tcp.SendData(packet);
        }

        public void SendDataUDP(TKey toClient, Packet packet)
        {
            SendUDPData(Holder.Get(toClient).Client.udp.EndPoint, packet);
        }

        protected abstract void OnNewTCPClientAdded(TcpClient client);

        protected abstract TKey GetIdFromPacket(Packet packet);

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
                Log.ZLogError($"Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }

        protected void ProcessUdpData(byte[] data, IPEndPoint clientEndPoint)
        {
            using (Packet packet = new Packet(data))
            {
                var gameRoomId = GetIdFromPacket(packet);

                if (gameRoomId.Equals(default(TKey)))
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

        protected void UDPReceiveCallback(IAsyncResult result)
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

        protected void TCPConnectCallback(IAsyncResult result)
        {
            var client = default(TcpClient);
            try
            {
                client = TcpListener.EndAcceptTcpClient(result);
                TcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            }
            catch (Exception ex) { }

            Log.ZLogInformation($"Incoming connection from {client.Client.RemoteEndPoint}...");

            if (IsAvailableToConnect)
            {
                OnNewTCPClientAdded(client);
                return;
            }

            Log.ZLogError($"Can't create game room, all ports is captured! can't connect game room {client.Client.RemoteEndPoint}...");
        }
    }
}
