using System.Net;
using System.Net.Sockets;

namespace GameServer.Network
{
    public class NetworkClient
    {
        public static int dataBufferSize = 4096;

        public Guid Id;
        public TCP tcp;
        public UDP udp;

        public NetworkClient(Guid clientId)
        {
            Id = clientId;
            tcp = new TCP(Id);
            udp = new UDP(Id);
        }

        public class TCP : IDisposable
        {
            public TcpClient Socket;
            public event Action<Guid, int, Packet> PacketReceived = delegate { };

            private readonly Guid _id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;
            private NetworkClient _client;

            public TCP(Guid id)
            {
                _id = id;
            }

            public void Connect(TcpClient socket, NetworkClient client)
            {
                Socket = socket;
                Socket.ReceiveBufferSize = dataBufferSize;
                Socket.SendBufferSize = dataBufferSize;

                stream = Socket.GetStream();
                _client = client;

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (Socket != null && Socket.Connected)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to player {_id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        _client?.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    _client?.Disconnect();
                    Console.WriteLine($"Error receiving TCP data: {ex}");
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();

                        PacketReceived(_id, packetId, packet);
                    }

                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Dispose()
            {
                stream.Dispose();
                Socket.Dispose();
            }
        }

        public class UDP
        {
            public IPEndPoint? EndPoint;
            public event Action<Guid, int, Packet> PacketReceived = delegate { };

            private readonly Guid _id;

            public UDP(Guid id)
            {
                _id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                EndPoint = endPoint;
            }

            public void HandleData(Packet packetData)
            {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);

                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    PacketReceived(_id, packetId, packet);
                }
            }
        }

        private void Disconnect()
        {
            Console.WriteLine($"{tcp.Socket.Client.RemoteEndPoint} was disconnected");

            tcp.Dispose();
        }
    }
}