using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;

namespace Lost_and_Found.Interfaces
{
    public interface IFindPhoneService
    {
        List<FindPhone> GetFoundedPhones();
        Task<FindPhone> AddFoundedPhone(FindPhoneDTO lostPhone);
        FindPhone UpdateFoundedPhone(FindPhoneDTO los);
        string DeleteFoundedPhone(string phonenum);
    }
}
