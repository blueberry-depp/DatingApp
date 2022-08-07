using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    // Helps us map from one object to another.
    // we're also going to be adding this as a dependency that we can inject, we need to add this to our application service extensions.
    public class AutoMapperProfiles : Profile
    {
        // Generate default constructor. 
        public AutoMapperProfiles()
        {
            // Where we want to map from and where we want to map to. In here AppUser to MemberDto.
            CreateMap<AppUser, MemberDto>()
                // ForMember means which property that we want to affect,
                // first parameter we pass is the destination, what property are we looking to affect,
                // next part of this is the options, where do we want this to map from and the source of where we're mapping from.
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                    src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                    src.DateOfBirth.CalculateAge()));

            CreateMap<Photo, PhotoDto>();
            // We're going to go the other way, from MemberUpdateDto to AppUser.
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();

            // Because there's a couple of properties or one property that we cannot
            // get automapper to do for us, and that's for the user photo.
            CreateMap<Message, MessageDto>()
               // This is for sender.
               // src =>: where we're mapping from. 
               .ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src =>
                    src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
               // This is for recipient.
               .ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src =>
                    src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
            //CreateMap<MessageDto, Message>();



        }
    }
}
