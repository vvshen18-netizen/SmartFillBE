using System.ComponentModel.DataAnnotations;

namespace SmarfillAPI.Models
{
    public class OrderRejection
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string DeliveryGuyEmail { get; set; }
    }
}
