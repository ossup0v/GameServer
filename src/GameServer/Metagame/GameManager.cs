using GameServer.Common;
using GameServer.Metagame.GameRooms;
using GameServer.Network;

namespace GameServer.Metagame
{
    public class GameManager : IGameManager
    {
        private readonly IServerSendToClient _serverSend;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoomManager _roomManager;

        public GameManager(IServerSendToClient serverSend, 
            IServiceProvider serviceProvider,
            IRoomManager roomManager)
        {
            _serverSend = serverSend;
            _serviceProvider = serviceProvider;
            _roomManager = roomManager;
        }

        public Task<ApiResult> JoinGameRoom(Guid roomId, MetagameUser user)
        {
            Console.WriteLine("User trying to join a game room");
            var room = _roomManager.GetRoom(roomId);
            if (room == null)
            {
                Console.WriteLine($"User {user?.Data?.Id}-{user?.Data?.Username} can't find room with id {roomId}");
                return Task.FromResult(ApiResult.Error("Can't create room"));
            }
            room.Join(user);
            _serverSend.RoomPortToConnect(user.Data.Id, room.Data.Port);
            Console.WriteLine($"Sended port {room.Data.Port} to user");

            return Task.FromResult(ApiResult.Ok);
        }

        public Task<ApiResult> CreateRoom(MetagameUser user, string mode, string title, int maxPlayerCount)
        {
            return Task.FromResult(_roomManager.CreateRoom(user, mode, title, maxPlayerCount));
        }

        public Task<ApiResult> SendRoomToUser(Guid userId)
        {
            _serverSend.RoomList(userId, _roomManager.Rooms);
            return Task.FromResult(ApiResult.Ok);
        }
    }
}
