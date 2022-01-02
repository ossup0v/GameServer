using GameServer.Metagame.GameRooms;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;

namespace GameServer.NetworkWrappper
{
    public class ServerSendToClient : ServerSendBase<IClientHolder, Guid, User>, IServerSendToClient
    {
        public ServerSendToClient(IClientHolder holder, IClientDataSender dataSender) : base(holder, dataSender) { }

        public void Welcome(Guid toClient, string msg)
        {
            using (Packet packet = new Packet(ToClient.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);
                packet.Write(1);

                SendTCPData(toClient, packet);
            }
        }

        public void RegisterResult(Guid toClient, Guid packetId, bool result)
        {
            using (Packet packet = new Packet(ToClient.response))
            {
                packet.Write(packetId);
                packet.Write(result);

                if (!result)
                    packet.Write("Can't register user. login already used.");

                SendTCPData(toClient, packet);
            }
        }

        public void LoginResult(Guid toClient, Guid packetId, bool result)
        {
            using (Packet packet = new Packet(ToClient.response))
            {
                packet.Write(packetId);
                packet.Write(result);

                if (!result)
                    packet.Write("Can't login user. wrong password or login.");

                SendTCPData(toClient, packet);
            }
        }

        public void RoomPortToConnect(Guid toClient, int port)
        {

            using (Packet packet = new Packet(ToClient.roomPortToConnect))
            {
#warning fix it, use config!
#if DEBUG
                packet.Write("127.0.0.1");
#else
                packet.Write("3.66.29.169");
#endif
                packet.Write(port);

                SendTCPData(toClient, packet);
            }
        }

        public void RoomList(Guid toClient, IEnumerable<GameRoom> rooms)
        {
            using (Packet packet = new Packet(ToClient.roomList))
            {
                packet.Write(rooms.Count());

                foreach (var room in rooms)
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
