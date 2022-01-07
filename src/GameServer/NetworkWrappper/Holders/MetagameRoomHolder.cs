using GameServer.Metagame.GameRooms.MetagameRooms;

namespace GameServer.NetworkWrappper.Holders
{
    public class MetagameRoomHolder : IMetagameRoomHolder
    {
        private Dictionary<Guid, MetagameGameRoom> _rooms = new Dictionary<Guid, MetagameGameRoom>();
        public int Count => _rooms.Count;

        public void AddNew(MetagameGameRoom @new)
        {
            _rooms.Add(@new.Id, @new);
        }

        public MetagameGameRoom? Get(Guid id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
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
