using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile // This is a class from AutoMapper nuget package that we installed
{
    // This is the class to Map Entities to DTOs and vice-versa 
    

    public AutoMapperProfiles()
    {
        //CreateMap<SOURCE, DESTINATION>();
        CreateMap<AppUser, MemberDto>()
        .ForMember(dest => dest.PhotoUrl, 
            opt => opt.MapFrom(src => src.Photos.FirstOrDefault(pic => pic.IsMain).Url));
        CreateMap<Photo, PhotoDto>();
    }
}
