﻿namespace BookSoulsApp.Domain.Entities;
public class Address
{
    public string Street { get; set; }
    public string Ward { get; set; }
    public string District { get; set; }
    public string City { get; set; }
    public string Country { get; set; } = "Vietnam"; // Default country
}
