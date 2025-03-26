using AutoMapper;
using UserProfile.Api.Models;
using UserProfile.Dal;

namespace UserProfile.Api.AutoMapper;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<RegisterUserDto, User>().ReverseMap();
        CreateMap<UpdateUserDto, User>().ReverseMap();
    }
}