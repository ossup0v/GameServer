using GameServer.Common;
using GameServer.NetworkWrappper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.Metagame.GameRooms.MetagameRooms
{
    public class MetagameGameRoom : IWithId<Guid>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerSendToClient _sendToClient;
        private readonly ILogger<MetagameGameRoom> _log;

        public Guid Id { get; }
        public int Port { get; }
        public Dictionary<Guid, MetagameUser> Users { get; set; } = new Dictionary<Guid, MetagameUser>();

        public MetagameGameRoom(Guid id, int port, IServiceProvider serviceProvider)
        {
            Id = id;
            Port = port;
            _serviceProvider = serviceProvider;
            _sendToClient = serviceProvider.GetRequiredService<IServerSendToClient>();
            _log = serviceProvider.GetRequiredService<ILogger<MetagameGameRoom>>();
        }

        public void Start()
        {
            var index = 0;
            foreach (var userId in Users.Keys)
            {
                _sendToClient.RoomPortToConnect(userId, GetUserTeam(index), Port);
                index++;
            }
        }

        public void Finish(GameRoomResult result)
        {
            foreach (var teamResult in result.TeamResult.Values)
            {
                _log.ZLogInformation($"Server receive game room results {teamResult}");
            }

            //TODO add to player inventory rewards
        }

        private int GetUserTeam(int index)
        {
            return (index % 2) + 1;
        }
    }
}
