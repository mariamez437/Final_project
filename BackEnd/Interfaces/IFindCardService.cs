using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;

namespace Lost_and_Found.Interfaces
{
    public interface IFindCardService
    {
        List<FindCard> GetFoundedCards();

        Task<FindCard> AddFoundedCard(FindCardDTO card);
        FindCard UpdateFoundedCard(FindCardDTO card);
        string DeleteFoundedCard(string cardid);
    }
}
