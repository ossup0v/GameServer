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
        public UserData Data;

        public MetagameUser(Guid id, IUserRepository userRepository, IClientHolder clientHolder, IMapper mapper)
        {
            Id = id;
            _userRepository = userRepository;
            _clientHolder = clientHolder;
            _mapper = mapper;
        }

        public void GetInventory()
        {

        }

        public async Task<ApiResult<UserData>> AutorizeUser(string login, string password)
        {
            var existsUser = await _userRepository.GetUserByLogin(login);

            if (existsUser.Password == password)
            {
                var user = new UserData
                {
                    Password = existsUser.Password,
                    Id = existsUser.Id,
                    Username = existsUser.Username,
                    Login = existsUser.Login,
                };

                return ApiResult<UserData>.OK(user);
            }
            else
            {
                return ApiResult<UserData>.Failed("Can't login user");
            }
        }

        public async Task<ApiResult> TryJoin(string login, string password)
        {
            var user = await AutorizeUser(login, password);
            if (user != null)
            {
                return ApiResult.Ok;
            }
            else
            {
                return ApiResult.Failed("Can't login user");
            }
        }

        public Task<ApiResult> Register(string login, string password, string username, Guid id)
        {
            var newUser = new UserModel { Login = login, Password = password, Username = username, Id = id };
            Data = _mapper.Map<UserData>(newUser);

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
                return ApiResult.Ok;
            }

            return ApiResult.Failed($"User with login {login}, not have password {password}");
        }
    }

    public class UserData
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsUserLogged { get; set; }
        public int Team { get; set; }
    }
}
