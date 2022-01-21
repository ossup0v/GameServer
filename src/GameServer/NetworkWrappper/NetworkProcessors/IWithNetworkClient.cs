using GameServer.Network;

namespace GameServer.NetworkWrappper
{
    public interface IWithNetworkClient
    {
        NetworkClient Client { get; }
    }
}
