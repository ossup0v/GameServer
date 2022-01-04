using GameServer.DAL.Models;

namespace GameServer.DAL.Interfaces
{
    public interface IUserRepository
    {
        Task AddUser(UserModel user);
        Task<List<UserModel>> GetAllUsers();
        Task<UserModel> GetUserByLogin(string login);
        Task<UserModel> GetUserById(Guid id);
    }
}
