namespace Lost_and_Found.Interfaces
{
    public interface IChecking_For_Items
    {
        public Task<object> All_Items(string email);
    }
}