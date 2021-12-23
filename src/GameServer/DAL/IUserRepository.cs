using GameServer.DAL.DTOs;

namespace GameServer.DAL
{
    public interface IUserRepository
    {
        Task AddUser(UserDTO user);
        Task<List<UserDTO>> GetAllUsers();
        Task<UserDTO> GetUserByLogin(string login);
        Task<UserDTO> GetUserById(Guid id);
    }
}
