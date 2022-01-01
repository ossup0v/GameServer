namespace GameServer.Network.Holders
{
    public class ClientHolder : IClientHolder
    {
        private Dictionary<Guid, User> _clients = new Dictionary<Guid, User>();
        public void AddNew(User newClient)
        {
            _clients.Add(newClient.Id, newClient);
        }

        public IEnumerable<User> GetAll()
        {
            return _clients.Values;
        }

        public User? Get(Guid clientId)
        {
            if (_clients.TryGetValue(clientId, out var client))
                return client;


            Console.WriteLine($"Can't find client with id {clientId}, all clients is {string.Join(" ", _clients.Keys)}");
            return null;
        }
    }

    public interface IClientHolder : IHolder<User, Guid>
    { }
}
