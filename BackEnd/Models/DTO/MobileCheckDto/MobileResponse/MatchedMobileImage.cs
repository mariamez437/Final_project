namespace Lost_and_Found.Models.DTO.MobileCheckDto.MobileResponse
{
    public class MatchedMobileImage
    {
        public string image_url { get; set; }
        public double image_similarity { get; set; }
        public FoundPhoneDto associated_data { get; set; }
    }
}
