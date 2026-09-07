// Step 2: Define the InventoryItem Class 

using System;
using System.ComponentModel.DataAnnotations;

namespace MSFD_LogiTrack.Models
{
    public class InventoryItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        public required string Name { get; set; }
        public int Quantity { get; set; }
        public required string Location { get; set; }


        // Foreign key to Order
        public int? OrderId { get; set; }
        public Order? Order { get; set; }


        // Display item information
        public void DisplayInfo()
        {
            Console.WriteLine($"Item: {Name} | Quantity: {Quantity} | Location: {Location}");
        }
    }
}