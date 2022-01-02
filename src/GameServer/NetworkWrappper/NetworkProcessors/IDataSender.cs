using GameServer.Network;

namespace GameServer.NetworkWrappper.NetworkProcessors
{
    public interface IDataSender<TClientId> where TClientId : struct
    {
        void SendDataUDP(TClientId toClient, Packet packet);
        void SendDataTCP(TClientId toClient, Packet packet);
    }
}
