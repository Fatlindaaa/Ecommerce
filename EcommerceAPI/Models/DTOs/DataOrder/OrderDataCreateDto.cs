﻿using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models.DTOs.DataOrder
{
    public class OrderDataCreateDto
    {
        [Required]
        public string PhoheNumber { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
