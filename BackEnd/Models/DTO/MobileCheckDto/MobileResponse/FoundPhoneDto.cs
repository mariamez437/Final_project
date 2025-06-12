using System.Drawing;
using System.IO;

namespace Lost_and_Found.Models.DTO.MobileCheckDto.MobileResponse
{
    public class FoundPhoneDto
    {
        public string governorate { get; set; }
        public string city { get; set; }
        public string street { get; set; }
        public string contact { get; set; }
        public string brand { get; set; }
        public string color { get; set; }
        public string image_url { get; set; }
    }
}
