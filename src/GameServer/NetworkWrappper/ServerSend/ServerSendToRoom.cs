using GameServer.Metagame.GameRooms;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;

namespace GameServer.NetworkWrappper
{
    public interface IServerSendToGameRoom 
    {
        void GameRoomData(Guid toGameRoom);
    }

    public class ServerSendToGameRoom : ServerSendBase<IGameRoomHolder, Guid, GameRoom>, IServerSendToGameRoom
    {
        public ServerSendToGameRoom(IGameRoomHolder holder, IGameRoomDataSender dataSender) : base(holder, dataSender) { }

        public void GameRoomData(Guid toGameRoom)
        {
            using (Packet packet = new Packet(ToGameRoom.gameRoomData))
            {
                //sending real id
                packet.Write(toGameRoom);

                SendTCPData(toGameRoom, packet);
            }
        }
    }
}
