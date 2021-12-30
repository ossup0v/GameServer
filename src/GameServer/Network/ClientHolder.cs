using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    public class ClientHolder : IClientHolder
    {
        private Dictionary<Guid, Client> _clients = new Dictionary<Guid, Client>();
        public void AddNewClient(Client newClient)
        {
            _clients.Add(newClient.Id, newClient);
        }

        public IEnumerable<Client> GetAllClients()
        {
            return _clients.Values;
        }

        public Client? GetClient(Guid clientId)
        {
            if (_clients.TryGetValue(clientId, out var client))
                return client;


            Console.WriteLine($"Can't find client with id {clientId}, all clients is {string.Join(" ", _clients.Keys)}");
            return null;
        }
    }

    public interface IClientHolder
    {
        Client? GetClient(Guid clientId);
        IEnumerable<Client> GetAllClients();
        void AddNewClient(Client newClient);

    }
}
