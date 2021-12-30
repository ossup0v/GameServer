using GameServer.Common;
using GameServer.Metagame.GameRoom;

namespace GameServer.Network
{
    public interface IServerSend
    {
        void RegisterResult(Guid fromClient, Guid packetId, bool result);
        void LoginResult(Guid fromClient, Guid packetId, bool result);
        void RoomPortToConnect(Guid id, int port);
        void RoomList(Guid userId, IReadOnlyDictionary<Guid, GameRoom> rooms);
        void Welcome(Guid newGuid, string v);
    }
}