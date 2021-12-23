using GameServer.Common;
using GameServer.Metagame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    public class Client
    {
        public static int dataBufferSize = 4096;

        public Guid id;
        public User User;
        public TCP tcp;
        public UDP udp;

        public Client(Guid clientId)
        {
            id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP : IDisposable
        {
            public TcpClient Socket;

            private readonly Guid _id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(Guid id)
            {
                _id = id;
            }

            public void Connect(TcpClient socket)
            {
                Socket = socket;
                Socket.ReceiveBufferSize = dataBufferSize;
                Socket.SendBufferSize = dataBufferSize;

                stream = Socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(_id, "Welcome to the server!");
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
                        Server.clients[_id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    Server.clients[_id].Disconnect();
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
                            Server.packetHandlers[packetId](_id, packet);
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

        public void CreateUser()
        {
            Console.WriteLine("User with id " + id + " created");
            User = new User();
        }

        public class UDP
        {
            public IPEndPoint EndPoint;

            private Guid Id;

            public UDP(Guid id)
            {
                Id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                EndPoint = endPoint;
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(EndPoint, packet);
            }

            public void HandleData(Packet packetData)
            {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);

                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](Id, packet);
                }
            }
        }

        public void SendIntoGame(string playerName)
        {
            Console.WriteLine("Send into game");

        }

        //private void StartRoomProcess()
        //{
        //    Process p = new Process();
        //    p.StartInfo.FileName = command;
        //    p.StartInfo.Arguments = arguments;
        //    p.StartInfo.RedirectStandardError = true;
        //    p.StartInfo.RedirectStandardOutput = true;
        //    p.StartInfo.CreateNoWindow = true;
        //    p.StartInfo.WorkingDirectory = Application.dataPath + "/..";
        //    p.StartInfo.UseShellExecute = false;
        //    p.Start();
        //}

        private void Disconnect()
        {
            Console.WriteLine($"{tcp.Socket.Client.RemoteEndPoint} was disconnected");

                User = null;

            tcp.Dispose();

            //ServerSend.PlayerDisconnected(id);
        }
    }
}