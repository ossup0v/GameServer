using AutoMapper;
using GameServer.DAL.DTOs;
using GameServer.Metagame;

namespace GameServer.Common
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserData, UserDTO>().ReverseMap();
        }
    }
}
