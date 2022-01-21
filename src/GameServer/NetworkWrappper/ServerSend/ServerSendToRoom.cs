using GameServer.Metagame.GameRooms;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;

namespace GameServer.NetworkWrappper
{
    public interface IServerSendToGameRoom 
    {
        void SendGameRoomData(Guid toGameRoom);
        void SendPlayersData(Guid toGameRoom, PlayerGameRoomData[] data);
    }

    public class ServerSendToGameRoom : ServerSendBase<IGameRoomHolder, Guid, GameRoom>, IServerSendToGameRoom
    {
        public ServerSendToGameRoom(IGameRoomHolder holder, IGameRoomDataSender dataSender) : base(holder, dataSender) { }

        public void SendGameRoomData(Guid toGameRoom)
        {
            using (Packet packet = new Packet(ToGameRoom.gameRoomData))
            {
                //sending real id
                packet.Write(toGameRoom);

                SendTCPData(toGameRoom, packet);
            }
        }

        public void SendPlayersData(Guid toGameRoom, PlayerGameRoomData[] data)
        {
            using (Packet packet = new Packet(ToGameRoom.playersData))
            {
                //amount of players
                packet.Write(data.Length);

                foreach (var player in data)
                {
                    packet.Write(player.Id);
                    packet.Write(player.Team);
                    packet.Write(player.UserName);
                }

                SendTCPData(toGameRoom, packet);
            }
        }
    }
}
