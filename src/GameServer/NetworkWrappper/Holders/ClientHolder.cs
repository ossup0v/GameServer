using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer.NetworkWrappper.Holders
{
    public class ClientHolder : IClientHolder
    {
        private readonly Dictionary<Guid, User> _clients = new Dictionary<Guid, User>();
        private readonly ILogger<ClientHolder> _log;

        public ClientHolder(ILogger<ClientHolder> log)
        {
            _log = log;
        }

        public int Count => _clients.Count;

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


            _log.ZLogError($"Can't find client with id {clientId}, all clients is {string.Join(" ", _clients.Keys)}");
            return null;
        }

        public void Remove(Guid key)
        {
            _clients.Remove(key);
        }
    }

    public interface IClientHolder : IHolder<Guid, User> { }
}
