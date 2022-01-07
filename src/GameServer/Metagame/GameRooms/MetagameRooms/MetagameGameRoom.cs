using GameServer.Common;
using GameServer.Configs;
using GameServer.NetworkWrappper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ZLogger;

namespace GameServer.Metagame.GameRooms.MetagameRooms
{
    public class MetagameGameRoom : IWithId<Guid>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerSendToClient _sendToClient;
        private readonly ILogger<MetagameGameRoom> _log;
        private readonly GameServerConfig _gameServerConfig;

        private bool _isReadyToStart => Users.Count >= Constants.CountOfPlayersToStartGameRoom;
        public Guid Id { get; }
        public string Mode { get; }
        public string Title { get; }
        public int Port { get; }
        public bool IsAvailableToJoin { get; private set; } = true;
        public Dictionary<Guid, MetagameUser> Users { get; set; } = new Dictionary<Guid, MetagameUser>();

        public MetagameGameRoom(Guid id, int port, string mode, string title, IServiceProvider serviceProvider)
        {
            Id = id;
            Mode = mode;
            Title = title;
            Port = port;
            _serviceProvider = serviceProvider;
            _sendToClient = serviceProvider.GetRequiredService<IServerSendToClient>();
            _log = serviceProvider.GetRequiredService<ILogger<MetagameGameRoom>>();
            _gameServerConfig= serviceProvider.GetRequiredService<GameServerConfig>();
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

        public Task<ApiResult> Join(MetagameUser user)
        {
            if (!IsAvailableToJoin)
            {
                _log.ZLogWarning($"User {user.Id} {user.Data?.Username} trying to join in room {Id}, but room is not available to join");

                return Task.FromResult(ApiResult.Failed("Can't join game"));
            }

            if (Users.ContainsKey(user.Id))
            {
                _log.ZLogWarning($"User {user.Id} {user.Data?.Username} trying to join in room {Id}, but already joined into game");

                return Task.FromResult(ApiResult.Failed("Already joined into game"));
            }

            Users.Add(user.Id, user);

            _log.ZLogInformation($"User {user.Id} {user.Data?.Username} joined to room {Id}. {Users.Count}/{Constants.CountOfPlayersToStartGameRoom}");

            if (_isReadyToStart)
                StartGameRoom();

            return Task.FromResult(ApiResult.Ok);
        }

        private void StartGameRoom()
        {
            IsAvailableToJoin = false;

            Process.Start(Constants.RoomExePath, GetGameRoomParams(
                Port,
                Id,
                Mode,
                Title,
                Constants.CountOfPlayersToStartGameRoom,
#warning тодо удалить это старьё
                Guid.NewGuid()));
        }

        private string GetGameRoomParams(int roomPort, Guid metagameRoomId, string mode, string title, int maxPlayerCount, Guid creatorId)
        {
            return $"{roomPort};{_gameServerConfig.GameRoomPort};{metagameRoomId};{mode};{title};{maxPlayerCount};{creatorId}";
        }

        public Task<ApiResult> Leave(MetagameUser user)
        {
            Users.Remove(user.Id);
            return Task.FromResult(ApiResult.Ok);

        }

        private int GetUserTeam(int index)
        {
            return (index % Constants.TeamCount) + 1;
        }
    }
}
