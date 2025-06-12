using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;

namespace Lost_and_Found.Interfaces
{
    public interface ILostPhoneService
    {
        List<LostPhone> GetLostPhones();
        List<int> GetLostPhonesOfID(string email);
        Task<LostPhone> AddLostPhone(LostPhoneDTO phone);
        LostPhone UpdateLostPhone(LostPhoneDTO ph);
        string DeleteLostPhone(string email, string phonenum);
        List<LostPhone> GetLostPhonesOfEmail(string email);
    }
}