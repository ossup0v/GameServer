using GameServer.DAL.DTOs;

namespace GameServer.DAL.Mongo
{
    internal class TempUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, UserDTO> _users = new Dictionary<Guid,UserDTO>();

        public Task AddUser(UserDTO user)
        {
            _users.Add(user.Id, user);

            return Task.CompletedTask;
        }

        public Task<List<UserDTO>> GetAllUsers()
        {
            return Task.FromResult(_users.Values.ToList());
        }

        public Task<UserDTO> GetUserById(Guid id)
        {
            _users.TryGetValue(id, out UserDTO user);
            return Task.FromResult(user);
        }

        public Task<UserDTO> GetUserByLogin(string login)
        {
            var user = _users.Values.FirstOrDefault(x => x.Login.Equals(login));
            return Task.FromResult(user);
        }
    }
}
