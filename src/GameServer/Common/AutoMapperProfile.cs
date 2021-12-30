using AutoMapper;
using GameServer.DAL.DTOs;
using GameServer.Metagame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
