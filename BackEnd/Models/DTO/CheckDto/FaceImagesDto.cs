using Newtonsoft.Json;

namespace Lost_and_Found.Models.DTO.CheckDto
{
    public class FaceImagesDto
    {
        [JsonProperty("lost_face")]
        public string lost_face { get; set; }

        [JsonProperty("found_face")]
        public string found_face { get; set; }
    }
}

