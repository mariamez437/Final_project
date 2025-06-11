using Lost_and_Found.Enums;
using MatchType = Lost_and_Found.Enums.MatchType;

namespace Lost_and_Found.Models.DTO.CheckDto
{


    public class MatchDto
    {
        public MatchType MatchType { get; set; }

        public string LostBrand { get; set; } = string.Empty;
        public string LostColor { get; set; } = string.Empty;
        public string LostGovernment { get; set; } = string.Empty;
        public string LostCenter { get; set; } = string.Empty;
        public string LostStreet { get; set; } = string.Empty;
        public string LostContact { get; set; } = string.Empty;

        public string FoundBrand { get; set; } = string.Empty;
        public string FoundColor { get; set; } = string.Empty;
        public string FoundGovernment { get; set; } = string.Empty;
        public string FoundCenter { get; set; } = string.Empty;
        public string FoundStreet { get; set; } = string.Empty;
        public string FoundContact { get; set; } = string.Empty;

        public byte[]? Image { get; set; }
    }
}
