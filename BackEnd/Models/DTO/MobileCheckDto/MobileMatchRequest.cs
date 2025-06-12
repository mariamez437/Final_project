using Lost_and_Found.Enums;
using Lost_and_Found.Models.DTO.CheckDto;
using MatchType = Lost_and_Found.Enums.MatchType;

namespace Lost_and_Found.Models.DTO.MobileCheckDto
{

    public class MobileMatchRequest
    {
        public MatchType MatchType { get; set; }
        public MobileLostMatchDto Lost { get; set; }
    }


}
