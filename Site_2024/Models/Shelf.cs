﻿namespace Site_2024.Web.Api.Models
{
    public class Shelf
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Aisle Aisle { get; set; }
    }
}
