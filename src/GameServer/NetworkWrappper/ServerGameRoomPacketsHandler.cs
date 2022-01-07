using GameServer.Metagame;
using GameServer.Metagame.GameRooms;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.NetworkWrappper
{
    public class ServerGameRoomPacketsHandler : IHostedService
    {
        private readonly IClientHolder _clientHolder;
        private readonly IGameRoomHolder _gameRoomHolder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerSendToClient _serverSend;
        private readonly IGameRoomDataReceiver _gameRoomDataReceiver;
        private readonly IGameManager _gameManager;
        private readonly IMetagameRoomHolder _metagameRoomHolder;
        private readonly ILogger<ServerGameRoomPacketsHandler> _log;

        public delegate Task PacketHandler(Guid fromClient, Packet packet);
        private Dictionary<int, PacketHandler> _handlers;

        private void InitializeHandlers()
        {
            _handlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ToServerFromGameRoom.gameRoomLaunched, GameRoomLaunched },
                { (int)ToServerFromGameRoom.gameSessionEnded, GameRoomEnd }
            };

            _log.ZLogInformation("Game room initialized packets.");
        }

        public ServerGameRoomPacketsHandler(IClientHolder clientHolder,
            IGameRoomHolder gameRoomHolder,
            IServiceProvider serviceProvider,
            IServerSendToClient serverSend,
            IGameRoomDataReceiver gameRoomDataReceiver,
            IGameManager gameManager,
            IMetagameRoomHolder metagameRoomHolder,
            ILogger<ServerGameRoomPacketsHandler> log)
        {
            _clientHolder = clientHolder;
            _gameRoomHolder = gameRoomHolder;
            _serviceProvider = serviceProvider;
            _serverSend = serverSend;
            _gameRoomDataReceiver = gameRoomDataReceiver;
            _gameManager = gameManager;
            _metagameRoomHolder = metagameRoomHolder;
            _log = log;
            InitializeHandlers();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _gameRoomDataReceiver.PacketReceived += OnPacketReceived;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _gameRoomDataReceiver.PacketReceived -= OnPacketReceived;

            return Task.CompletedTask;
        }

        private void OnPacketReceived(Guid fromGameRoom, int packetId, Packet packet)
        {
            if (_handlers.TryGetValue(packetId, out var packetHandler))
            {
                packetHandler(fromGameRoom, packet);
            }
            else
            {
                _log.ZLogError($"Error! can't process packer with packet id {packetId}, from game room {fromGameRoom}");
            }
        }

        private Task GameRoomLaunched(Guid fromGameRoom, Packet packet)
        {
            _log.ZLogInformation("Game room laucnhed !");

            var roomIdCheck = packet.ReadGuid();

            _log.ZLogInformation($"Welcome received from id on server {fromGameRoom}, in packet {roomIdCheck}");
            _log.ZLogInformation($"{_gameRoomHolder.Get(fromGameRoom)?.Client.tcp.Socket.Client.RemoteEndPoint} connected successfully and is now game room {fromGameRoom}.");

            if (fromGameRoom != roomIdCheck)
            {
                _log.ZLogError($"Player (ID: {fromGameRoom}) has assumed the wrong client ID ({roomIdCheck})!");
            }

            var metagameRoomId = packet.ReadGuid();
            var creatorId = packet.ReadGuid();
            var mode = packet.ReadString();
            var title = packet.ReadString();
            var maxPlayerCount = packet.ReadInt();
            var gameRoomPort = packet.ReadInt();

            var creatorOfGameRoom = _clientHolder.Get(creatorId)?.MetagameUser;

            if (creatorOfGameRoom == null)
            {
                _log.ZLogError($"Error! can't find creator of game room! Mode:{mode} Title:{title} MaxPlayers:{maxPlayerCount} RoomId:{fromGameRoom}");
            }

            _gameRoomHolder.Get(fromGameRoom).Data = new GameRoomData
            {
                Title = title,
                Mode = mode,
                MaxPlayerCount = maxPlayerCount,
                RoomId = fromGameRoom,
                Creator = creatorOfGameRoom,
                Port = gameRoomPort
            };

            _metagameRoomHolder.Get(metagameRoomId).Start();

            if (creatorOfGameRoom != null)
                _gameManager.JoinGameRoom(fromGameRoom, creatorOfGameRoom);

            return Task.CompletedTask;
        }

        private Task GameRoomEnd(Guid fromGameRoom, Packet packet)
        {
            var port = packet.ReadInt();
            var metagameRoomId = packet.ReadGuid();
            var teamsCount = packet.ReadInt();
            var teamScores = new TeamScore[teamsCount];

            for (var i = 0; i < teamScores.Length; i++)
            {
                var playerCount = packet.ReadInt();
                var playerIds = new Guid[playerCount];

                teamScores[i] = new TeamScore();

                for (int j = 0; j < playerIds.Length; j++)
                    playerIds[j] = packet.ReadGuid();


                teamScores[i] = new TeamScore
                {
                    PlayerIds = playerIds,
                    Team = packet.ReadInt(),
                    Plase = packet.ReadInt(),
                    KilledMobs = packet.ReadInt(),
                    KilledPlayers = packet.ReadInt(),
                    DeadPlayers = packet.ReadInt()
                };
            }

            var metagameRoom = _metagameRoomHolder.Get(metagameRoomId);

            if (metagameRoom == null)
            {
                _log.ZLogError($"{nameof(GameRoomEnd)} can't find metagame room");
                return Task.CompletedTask;
            }

            metagameRoom.Finish(new GameRoomResult
            {
                TeamResult = teamScores.ToDictionary(x => x.Team, x => x)
            });

            _metagameRoomHolder.Remove(metagameRoomId);

            _gameManager.GameRoomSessionEnd(port);

            _gameRoomHolder.Remove(fromGameRoom);

            return Task.CompletedTask;
        }
    }
}
