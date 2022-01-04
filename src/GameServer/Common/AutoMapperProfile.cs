using AutoMapper;
using GameServer.DAL.Models;
using GameServer.Metagame;

namespace GameServer.Common
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserData, UserModel>().ReverseMap();
        }
    }
}
