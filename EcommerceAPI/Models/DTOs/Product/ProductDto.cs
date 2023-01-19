﻿using Nest;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }

        [Required, StringLength(100), Display(Name = "Name")]
        public string Title { get; set; }


        [Required, StringLength(10000), Display(Name = "Product Description"), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Price")]
        public double Price { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int CoverTypeId { get; set; }
    }
}