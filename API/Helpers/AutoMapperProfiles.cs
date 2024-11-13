using API.DTOs;
using API.Entities;
using API.Extensions;
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
                opt => opt.MapFrom(src => src.Photos.FirstOrDefault(pic => pic.IsMain).Url))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<Message, MessageDto>()
            .ForMember(d => d.SenderPhotoUrl, 
                opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(pic => pic.IsMain).Url))
            .ForMember(d => d.RecipientPhotoUrl,
                opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(pic => pic.IsMain).Url));
    }
}
