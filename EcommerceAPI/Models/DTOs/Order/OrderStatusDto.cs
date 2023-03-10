using EcommerceAPI.Models.Entities;

namespace EcommerceAPI.Models.DTOs.Order
{
    public class OrderStatusDto
    {
        public string OrderId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

        public string Email { get; set; }
    }
}
