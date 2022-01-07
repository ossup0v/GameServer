using AutoMapper;
using GameServer.Common;
using GameServer.DAL.Interfaces;
using GameServer.DAL.Models;
using GameServer.NetworkWrappper.Holders;

namespace GameServer.Metagame
{
    public class MetagameUser
    {
        private readonly IUserRepository _userRepository;
        public readonly Guid Id;
        private readonly IClientHolder _clientHolder;
        private readonly IMapper _mapper;
        public UserData Data = new UserData();

        public MetagameUser(Guid id, IUserRepository userRepository, IClientHolder clientHolder, IMapper mapper)
        {
            Id = id;
            _userRepository = userRepository;
            _clientHolder = clientHolder;
            _mapper = mapper;
        }

        public Task<ApiResult> Register(string login, string password, string username, Guid id)
        {
            var newUser = new UserModel { Login = login, Password = password, Username = username, Id = id };
            Data = _mapper.Map<UserData>(newUser);
            
            UserJoined();
            
            _userRepository.AddUser(newUser);
            return Task.FromResult(ApiResult.Ok);
        }

        public async Task<ApiResult> Login(string login, string password, Guid id)
        {
            var existsUser = await _userRepository.GetUserByLogin(login);

            if (existsUser == null)
            {
                return ApiResult.Failed($"Can't find user with login {login}");
            }

            if (existsUser.Password == password)
            {
                Data = _mapper.Map<UserData>(existsUser);
                _clientHolder.Get(id).MetagameUser = this;
                UserJoined();
                return ApiResult.Ok;
            }

            return ApiResult.Failed($"User with login {login}, not have password {password}");
        }

        private void UserJoined()
        {
            Data.IsUserLogged = true;
        }

        private void UserExit()
        {
            Data.IsUserLogged = false;
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
