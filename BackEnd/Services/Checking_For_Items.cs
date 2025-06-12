using Lost_and_Found.Interfaces;
using Lost_and_Found.Models;
using Lost_and_Found.Models.Entites;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace Lost_and_Found.Services
{
    public class Checking_For_Items : IChecking_For_Items
    {
        private readonly DataConnection con;
        public Checking_For_Items(DataConnection con)
        {
            this.con = con;
        }
        public async Task<object> All_Items(string email)
        {
            List<LostCard> lst = await con.LostCards.Where(o => o.ForiegnKey_UserEmail == email).ToListAsync();

            List<object> ret = [];
            foreach (var item in lst)
                if ( await con.FindCards.AnyAsync(o => o.CardID == item.CardID))
                {
                    ret.Add(new
                    {
                        Found_Card = item.CardID,
                        Founder_Email = con.FindCards.FirstOrDefault(o => o.CardID == item.CardID)?.FinderEmail
                    });  
                }

            List<LostPhone> lst2 = await con.LostPhones.Where(o => o.ForiegnKey_UserEmail == email).ToListAsync();

            foreach (var item in lst2)
                if (await con.FindPhones.AnyAsync(o => o.Brand == item.Brand))
                {
                    ret.Add(new
                    {
                        Founder_Email = con.FindPhones.FirstOrDefault(o => o.Brand == item.Brand)?.FinderEmail
                    });
                    
                }

            return  ret;
        }
    }
}