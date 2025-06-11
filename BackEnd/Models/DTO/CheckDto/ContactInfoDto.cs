using Newtonsoft.Json;

namespace Lost_and_Found.Models.DTO.CheckDto
{
    public class ContactInfoDto
    {
        [JsonProperty("found")]
        public string Found { get; set; }
    }
}
