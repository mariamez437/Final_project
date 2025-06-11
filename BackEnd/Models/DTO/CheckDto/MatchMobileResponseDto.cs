using Lost_and_Found.Enums;
namespace Lost_and_Found.Models.DTO.CheckDto
{
    public class MatchMobileResponseDto
    {
        public string MatchType { get; set; }
        public List<MatchedMobileImageDto> MatchedImages { get; set; }
        public double ImageSimilarity { get; set; }
        public double TextSimilarity { get; set; }
        public double FinalScore { get; set; }
        public bool Matched { get; set; }
    }
}
