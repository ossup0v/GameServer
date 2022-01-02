using GameServer.Metagame.GameRooms;

namespace GameServer.NetworkWrappper.Holders
{
    public class GameRoomHolder : IGameRoomHolder
    {
        private Dictionary<Guid, GameRoom> _gameRooms = new Dictionary<Guid, GameRoom>();

        public int Count => _gameRooms.Count;
     
        public void AddNew(GameRoom @new)
        {
            _gameRooms.Add(@new.Id, @new);
        }

        public GameRoom? Get(Guid id)
        {

            if (_gameRooms.TryGetValue(id, out var room))
                return room;


            Console.WriteLine($"Can't find room with id {id}, all rooms is {string.Join(" ", _gameRooms.Keys)}");
            return null;
        }

        public IEnumerable<GameRoom> GetAll()
        {
            return _gameRooms.Values;
        }
    }

    public interface IGameRoomHolder : IHolder<Guid, GameRoom> { }
}
