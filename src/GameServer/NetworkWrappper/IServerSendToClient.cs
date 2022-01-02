using GameServer.Metagame.GameRooms;

namespace GameServer.NetworkWrappper
{
    public interface IServerSendToClient
    {
        void RegisterResult(Guid fromClient, Guid packetId, bool result);
        void LoginResult(Guid fromClient, Guid packetId, bool result);
        void RoomPortToConnect(Guid id, int port);
        void RoomList(Guid userId, IEnumerable<GameRoom> rooms);
        void Welcome(Guid newGuid, string message);
    }
}