using Newtonsoft.Json;

namespace Lost_and_Found.Models.DTO.CheckDto
{
    public class MatchCardResponse
    {

        [JsonProperty("text_similarity")]
        public double? text_similarity { get; set; }

        [JsonProperty("face_verified")]
        public bool? face_verified { get; set; }

        [JsonProperty("face_distance")]
        public double? face_distance { get; set; }

        [JsonProperty("match_result")]
        public bool match_result { get; set; }

        [JsonProperty("face_images")]
        public FaceImagesDto face_images { get; set; }

        [JsonProperty("contact_info")]
        public ContactInfoDto contact_info { get; set; }
    }
}
