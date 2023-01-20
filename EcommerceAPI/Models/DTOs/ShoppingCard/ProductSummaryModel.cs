﻿using EcommerceAPI.Models.DTOs.Order;

namespace EcommerceAPI.Models.DTOs.ShoppingCard
{
    public class ProductSummaryModel
    {
        public AddressDetails AddressDetails { get; set; }

        public List<ShoppingCardViewDto> ShoppingCardItems { get; set; }
    }
}
