using GameServer.Metagame.Room;
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

        public static void RegisterResult(Guid toClient, Guid packetId, bool result)
        {
            using (Packet packet = new Packet(ServerPackets.response))
            {
                packet.Write(packetId);
                packet.Write(result);

                if (!result)
                    packet.Write("Can't register user. login already used.");

                SendTCPData(toClient, packet);
            }
        }

        public static void LoginResult(Guid toClient, Guid packetId, bool result)
        {
            using (Packet packet = new Packet(ServerPackets.response))
            {
                packet.Write(packetId);
                packet.Write(result);

                if (!result)
                    packet.Write("Can't login user. wrong password or login.");

                SendTCPData(toClient, packet);
            }
        }

        public static void RoomPortToConnect(Guid toClient, int port)
        {

            using (Packet packet = new Packet(ServerPackets.roomPortToConnect))
            {
#if DEBUG
                packet.Write("127.0.0.1");
#else
                packet.Write("3.66.29.169");
#endif
                packet.Write(port);

                SendTCPData(toClient, packet);
            }
        }

        public static void RoomList(Guid toClient, IReadOnlyDictionary<Guid, Room> rooms)
        {
            using (Packet packet = new Packet(ServerPackets.roomList))
            {
                packet.Write(rooms.Count);

                foreach (var room in rooms.Values)
                {
                    packet.Write(room.Data.RoomId);
                    packet.Write(room.Data.Port);
                    packet.Write(room.Data.Creator?.Data?.Username ?? "kto ento");
                    packet.Write(room.Data.Mode);
                    packet.Write(room.Data.Title);
                    packet.Write(room.Data.MaxPlayerCount);
                    packet.Write(room.Data.Users.Count);
                }

                SendTCPData(toClient, packet);
            }
        }
    }
}
