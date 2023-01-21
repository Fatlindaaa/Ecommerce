﻿using AutoMapper;
using EcommerceAPI.Models.DTOs.Category;
using EcommerceAPI.Models.DTOs.CoverType;
using EcommerceAPI.Models.DTOs.Order;
using EcommerceAPI.Models.DTOs.Product;
using EcommerceAPI.Models.DTOs.Review;
using EcommerceAPI.Models.Entities;

namespace EcommerceAPI.Helpers
{
    public class AutoMapperConfigurations : Profile
    {
        public AutoMapperConfigurations()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductCreateDto>().ReverseMap();

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CategoryCreateDto>().ReverseMap();

            //CreateMap<OrderDetails, OrderDetailsCreateDto>().ReverseMap();
            //CreateMap<OrderData, OrderDataCreateDto>().ReverseMap();
            
            CreateMap<Review, ReviewCreateDto>().ReverseMap();
        }
    }
}
