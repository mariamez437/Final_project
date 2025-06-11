using Newtonsoft.Json;

namespace Lost_and_Found.Models.DTO.CheckDto
{
    public class MatchCardResponse
    {
        [JsonProperty("text_similarity")]
        public double? TextSimilarity { get; set; }

        [JsonProperty("face_verified")]
        public bool FaceVerified { get; set; }

        [JsonProperty("face_distance")]
        public double FaceDistance { get; set; }

        [JsonProperty("match_result")]
        public bool MatchResult { get; set; }

        [JsonProperty("face_images")]
        public FaceImagesDto FaceImages { get; set; }

        [JsonProperty("contact_info")]
        public ContactInfoDto ContactInfo { get; set; }
    }
}
