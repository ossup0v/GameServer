using GameServer.Common;
using GameServer.Metagame.GameRoom;
using GameServer.Network;

namespace GameServer.Metagame
{
    public class GameManager : IGameManager
    {
        private readonly IServerSend _serverSend;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoomManager _roomManager;

        public GameManager(IServerSend serverSend, 
            IServiceProvider serviceProvider,
            IRoomManager roomManager)
        {
            _serverSend = serverSend;
            _serviceProvider = serviceProvider;
            _roomManager = roomManager;
        }

        public Task<ApiResult> JoinGameRoom(Guid roomId, User user)
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

        public async Task<ApiResult> CreateRoom(User user, string mode, string title, int maxPlayerCount)
        {
            var room = _roomManager.CreateRoom(user, mode, title, maxPlayerCount);

            if (room == null)
            {
                return ApiResult.Error("Can't create room");
            }

            room.Join(user);
#warning не, нужно пофиксить это, а то полное г......вно...
            await Task.Delay((int)TimeSpan.FromSeconds(1).TotalMilliseconds);

            _serverSend.RoomPortToConnect(user.Data.Id, room.Data.Port);

            return ApiResult.Ok;
        }

        public Task<ApiResult> SendRoomToUser(Guid userId)
        {
            _serverSend.RoomList(userId, _roomManager.Rooms);
            return Task.FromResult(ApiResult.Ok);
        }
    }
}
