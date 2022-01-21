using GameServer.Metagame.GameRooms;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace GameServer.NetworkWrappper.NetworkProcessors
{
    public class GameRoomNetworkProcessor : NetworkProcessorBase<IGameRoomHolder, Guid, GameRoom>, IGameRoomDataSender, IGameRoomDataReceiver
    {
        public GameRoomNetworkProcessor(IGameRoomHolder gameRoomHolder, IServiceProvider serviceProvider, ILogger<GameRoomNetworkProcessor> log) 
            : base(gameRoomHolder, serviceProvider, log) { }
        
        protected override Guid GetIdFromPacket(Packet packet)
        {
            return packet.ReadGuid();
        }

        protected override void OnNewTCPClientAdded(TcpClient client)
        {
            var newGameRoomId = Guid.NewGuid();
            var newGameRoom = new GameRoom(newGameRoomId, ServiceProvider);

            newGameRoom.Connect(client);
            newGameRoom.SubsctibeToReceivePackets(PacketReceived);

            Holder.AddNew(newGameRoom);

            NewNetworkClientAdded(newGameRoomId);
        }
    }
 
    public interface IGameRoomDataSender : IDataSender<Guid> { }

    public interface IGameRoomDataReceiver : IDataReceiver<Guid> { }
}
