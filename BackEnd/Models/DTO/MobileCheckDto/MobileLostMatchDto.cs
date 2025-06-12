namespace Lost_and_Found.Models.DTO.MobileCheckDto
{
    public class MobileLostMatchDto
    {
        public string governorate { get; set; }
        public string city { get; set; }
        public string street { get; set; }
        public string contact { get; set; }
        public string image_name { get; set; }
        public string brand { get; set; }
        public string color { get; set; }

        public byte[] image { get; set; }
    }
}
