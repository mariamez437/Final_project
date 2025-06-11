using Lost_and_Found.Enums;
using Lost_and_Found.Models.DTO.CheckDto;
using MatchType = Lost_and_Found.Enums.MatchType;

namespace Lost_and_Found.Models.DTO
{

    public class CardMatchRequest
    {
        public MatchType MatchType { get; set; }
        public CardLostMatchDto Lost { get; set; }
    }

   
}
