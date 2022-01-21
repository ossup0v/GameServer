using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace GameServer.NetworkWrappper.NetworkProcessors
{
    public class ClientNetworkProcessor : NetworkProcessorBase<IClientHolder, Guid, User>, IClientDataSender, IClientDataReceiver
    {
        public ClientNetworkProcessor(IClientHolder clientHolder, IServiceProvider serviceProvider, ILogger<ClientNetworkProcessor> log) 
            : base(clientHolder, serviceProvider, log) { }

        protected override Guid GetIdFromPacket(Packet packet)
        {
            return packet.ReadGuid();
        }

        protected override void OnNewTCPClientAdded(TcpClient client)
        {
            var userId = Guid.NewGuid();
            var newUser = new User(userId, ServiceProvider.GetRequiredService<ILogger<User>>(), ServiceProvider);

            newUser.Connect(client);
            newUser.SubsctibeToReceivePackets(PacketReceived);

            Holder.AddNew(newUser);

            NewNetworkClientAdded(userId);
        }
    }

    public interface IClientDataSender : IDataSender<Guid> { }

    public interface IClientDataReceiver : IDataReceiver<Guid> { }
}
