using AutoMapper;
using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;

namespace Lost_and_Found.Helpers
{
    public class Mapper : Profile
    {
        public Mapper()
        {
           //CreateMap<LostCard, LostCardsDTO>();
           //  CreateMap<LostPhone, LostPhoneDTO>();
           // CreateMap<LostCard, FindCardDTO>();
           // CreateMap<User, RegisterDTO>();
           // CreateMap<CardsDTO, LostCard>();
           //      .ForMember(o=>o.CardPhoto,o2 => o2.Ignore());

            /*CreateMap<PhoneDTO, LostPhone>();
                .ForMember(o => o.PhonePhoto, o2 => o2.Ignore());*/
        }
    }
}
