using GameServer.Network;

namespace GameServer.NetworkWrappper.NetworkProcessors
{
    public interface IDataReceiver<TClientId> where TClientId : struct 
    {
        void StartReceive(int port, int maxPlayers);
        void StopReceive();
        
        Action<TClientId> NewNetworkClientAdded { get; set; }
        Action<TClientId, int, Packet> PacketReceived { get; set; }
    }
}
