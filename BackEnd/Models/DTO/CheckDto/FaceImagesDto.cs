using Newtonsoft.Json;

namespace Lost_and_Found.Models.DTO.CheckDto
{
    public class FaceImagesDto
    {
        [JsonProperty("lost_face")]
        public string LostFace { get; set; }

        [JsonProperty("found_face")]
        public string FoundFace { get; set; }
    }
}

