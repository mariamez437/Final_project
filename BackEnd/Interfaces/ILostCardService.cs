using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;

namespace Lost_and_Found.Interfaces
{
    public interface ILostCardService
    {
        List<LostCard> GetLostCards();
        List<LostCard> GetLostCardsOfEmail(string email);
        Task<LostCard?> AddLostCard(LostCardsDTO lostCardDTO);
        LostCard UpdateLostCard(LostCardsDTO card);
        public string DeleteLostCard(string email, string CardID);

    }
}
