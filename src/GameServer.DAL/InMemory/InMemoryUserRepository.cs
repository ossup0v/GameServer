using GameServer.DAL.Interfaces;
using GameServer.DAL.Models;

namespace GameServer.DAL.InMemory
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, UserModel> _users = new Dictionary<Guid, UserModel>();

        public Task AddUser(UserModel user)
        {
            _users.Add(user.Id, user);

            return Task.CompletedTask;
        }

        public Task<List<UserModel>> GetAllUsers()
        {
            return Task.FromResult(_users.Values.ToList());
        }

        public Task<UserModel> GetUserById(Guid id)
        {
            _users.TryGetValue(id, out UserModel user);
            return Task.FromResult(user);
        }

        public Task<UserModel> GetUserByLogin(string login)
        {
            var user = _users.Values.FirstOrDefault(x => x.Login.Equals(login));
            return Task.FromResult(user);
        }
    }
}
