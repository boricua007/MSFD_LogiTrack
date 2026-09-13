using System.ComponentModel.DataAnnotations;

namespace MSFD_LogiTrack.DTOs
{
    public class ItemDto
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }
    }
}
