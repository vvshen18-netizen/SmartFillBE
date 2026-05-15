using System.ComponentModel.DataAnnotations;

namespace SmarfillAPI.Models
{
    public class FuelPrice
    {
        public int Id { get; set; }
        public decimal Ron95Price { get; set; }
        public decimal Ron97Price { get; set; }
        public decimal DieselPrice { get; set; }
    }

}
