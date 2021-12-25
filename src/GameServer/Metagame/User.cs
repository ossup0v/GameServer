using GameServer.DAL;
using GameServer.DAL.DTOs;
using GameServer.DAL.Mongo;
using GameServer.Network;

namespace GameServer.Metagame
{
    public class User
    {
        private readonly IUserRepository _userRepository;

        public UserData Data;

        public User(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User(UserData data)
        {
            Data = data;
        }

        public void GetInventory()
        { 
        
        }

        public async Task<UserData> AutorizeUser(string login, string password)
        {
            var existsUser = await _userRepository.GetUserByLogin(login);

            var user = default(UserData);
            if (existsUser.Password == password)
            {
                user = new UserData
                {
                    Password = existsUser.Password,
                    Id = existsUser.Id,
                    Username = existsUser.Username,
                    Login = existsUser.Login,
                };
            }
            else
            {
                user = null;
            }

            return user;
        }

        public async Task<bool> TryJoin(string login, string password)
        {
            var user = await AutorizeUser(login, password);
            if (user != null)
            {

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Register(string login, string password, string username, Guid id)
        {
            var newUser = new UserDTO { Login = login, Password = password, Username = username, Id = id };
            Data = new UserData { Login = login, Password = password, Username = username, Id = id };

            _userRepository.AddUser(newUser);
            return true;
        }

        public async Task<bool> Login(string login, string password, Guid id)
        {
            var existsUser = await _userRepository.GetUserByLogin(login);

            if (existsUser == null)
            {
                return false;
            }

            if (existsUser.Password == password)
            {
                Data = new UserData { Password = existsUser.Password, Id = id, Login = existsUser.Login, Username = existsUser.Username };
                Server.GetClient(id).User = this;
                return true;
            }

            return false;
        }
    }

    public class UserData
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsUserLogged { get; set; }
    }
}
