﻿using System.ComponentModel.DataAnnotations;

namespace Site_2024.Web.Api.Requests
{
    public class ShelfAddRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        public int AisleId { get; set; }
    }
}
