using AutoMapper;
using UserProfile.Api.Models;
using Users.Dal.Entities;

namespace UserProfile.Api.AutoMapper;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<RegisterUserDto, User>().ReverseMap();
        CreateMap<UpdateUserDto, User>().ReverseMap();
        CreateMap<ProfileDto, ProfileInfo>()
            .ForMember(d => d.UserId, s => s.Ignore())
            .ReverseMap();
    }
}