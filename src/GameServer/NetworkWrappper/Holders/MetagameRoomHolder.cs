using GameServer.Metagame.GameRooms.MetagameRooms;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.NetworkWrappper.Holders
{
    public class MetagameRoomHolder : IMetagameRoomHolder
    {
        private Dictionary<Guid, MetagameGameRoom> _rooms = new Dictionary<Guid, MetagameGameRoom>();
        private readonly ILogger<MetagameGameRoom> _log;

        public MetagameRoomHolder(ILogger<MetagameGameRoom> log)
        {
            _log = log;
        }
        
        public int Count => _rooms.Count;

        public void AddNew(MetagameGameRoom @new)
        {
            _rooms.Add(@new.Id, @new);
        }

        public MetagameGameRoom Get(Guid id)
        {
            if (_rooms.TryGetValue(id, out var room))
                return room;


            _log.ZLogError($"Can't find metagame room with id {id}, all rooms is {string.Join(" ", _rooms.Keys)}, call stack {Environment.StackTrace}");
            return null;
        }

        public IEnumerable<MetagameGameRoom> GetAll()
        {
            return _rooms.Values;
        }

        public void Remove(Guid key)
        {
            _rooms.Remove(key);
        }
    }

    public interface IMetagameRoomHolder : IHolder<Guid, MetagameGameRoom> { }
}
