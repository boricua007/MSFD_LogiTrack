using System; 
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MSFD_LogiTrack.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public required string CustomerName { get; set; }

        public DateTime DatePlaced { get; set; } = DateTime.Now;

        // List of items in the order
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();


        // Add an item to the order
        public void AddItem(InventoryItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.ItemId != 0 && Items.Any(i => i.ItemId == item.ItemId)) return; // skip duplicates
            Items.Add(item);
        }


        // Remove an item by ItemId
        public void RemoveItem(int itemId)
        {
            var item = Items.Find(i => i.ItemId == itemId);
            if (item != null)
            {
                Items.Remove(item);
            }            
        }

        
        // Get a summary of the order
        public string GetOrderSummary()
        {
            var itemNames = string.Join(", ", Items.Select(i => i.Name));
            return $"Order #{OrderId} for {CustomerName} | Items: {Items.Count} ({itemNames}) | Placed: {DatePlaced:d}";
        }


    }
}