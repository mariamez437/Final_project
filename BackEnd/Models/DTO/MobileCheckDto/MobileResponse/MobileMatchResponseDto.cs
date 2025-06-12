namespace Lost_and_Found.Models.DTO.MobileCheckDto.MobileResponse
{
    public class MobileMatchResponseDto
    {
        public string match_type { get; set; }
        public int matched { get; set; }
        public double final_score { get; set; }
        public double text_similarity { get; set; }
        public FoundPhoneDto text_best_match { get; set; }
        public double image_similarity { get; set; }
        public List<MatchedMobileImage> matched_images { get; set; }
    }
}
