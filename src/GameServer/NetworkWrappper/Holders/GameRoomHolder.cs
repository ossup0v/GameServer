using GameServer.Metagame.GameRooms;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.NetworkWrappper.Holders
{
    public class GameRoomHolder : IGameRoomHolder
    {
        private readonly Dictionary<Guid, GameRoom> _gameRooms = new Dictionary<Guid, GameRoom>();
        private readonly ILogger<GameRoomHolder> _log;

        public GameRoomHolder(ILogger<GameRoomHolder> log)
        {
            _log = log;
        }

        public int Count => _gameRooms.Count;
     
        public void AddNew(GameRoom @new)
        {
            _gameRooms.Add(@new.Id, @new);
        }

        public GameRoom Get(Guid id)
        {
            if (_gameRooms.TryGetValue(id, out var room))
                return room;

            _log.ZLogError($"Can't find game room with id {id}, all rooms is {string.Join(" ", _gameRooms.Keys)}, call stack {Environment.StackTrace}"); 
            return null;
        }

        public IEnumerable<GameRoom> GetAll()
        {
            return _gameRooms.Values;
        }

        public void Remove(Guid key)
        {
            _gameRooms.Remove(key);
        }
    }

    public interface IGameRoomHolder : IHolder<Guid, GameRoom> { }
}
