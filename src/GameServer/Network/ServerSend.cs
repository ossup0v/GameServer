using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    public class ServerSend
    {
        #region SendBase
        private static void SendTCPData(Guid toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendUDPData(Guid toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.clients.Values)
            {
                client.tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAll(Guid exceptClient, Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.clients.Values)
            {
                if (client.id != exceptClient)
                {
                    client.tcp.SendData(packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.clients.Values)
            {
                client.udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(Guid exceptClient, Packet packet)
        {
            packet.WriteLength();

            foreach (var client in Server.clients.Values)
            {
                if (client.id != exceptClient)
                {
                    client.udp.SendData(packet);
                }
            }
        }
        #endregion

        public static void Welcome(Guid toClient, string msg)
        {
            using (Packet packet = new Packet(ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);
                packet.Write(1);

                SendTCPData(toClient, packet);
            }
        }

        public static void RoomPortToConnect(Guid toClient, int port)
        {

            using (Packet packet = new Packet(ServerPackets.roomPortToConnect))
            {
                packet.Write("127.0.0.1");
                packet.Write(port);

                SendTCPData(toClient, packet);
            }
        }
    }
}
