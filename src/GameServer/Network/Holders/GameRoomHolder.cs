using GameServer.Metagame.GameRooms;

namespace GameServer.Network.Holders
{
    public class GameRoomHolder : IGameRoomHolder
    {
        private Dictionary<Guid, GameRoom> _rooms = new Dictionary<Guid, GameRoom>();

        public void AddNew(GameRoom @new)
        {
            _rooms.Add(@new.Id, @new);
        }

        public GameRoom? Get(Guid id)
        {

            if (_rooms.TryGetValue(id, out var room))
                return room;


            Console.WriteLine($"Can't find room with id {id}, all rooms is {string.Join(" ", _rooms.Keys)}");
            return null;
        }

        public IEnumerable<GameRoom> GetAll()
        {
            return _rooms.Values;
        }
    }

    public interface IGameRoomHolder : IHolder<GameRoom, Guid> { }
}
