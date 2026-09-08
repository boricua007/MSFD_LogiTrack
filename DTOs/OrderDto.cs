
namespace MSFD_LogiTrack.DTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public List<ItemDto> Items { get; set; }
    }
}




