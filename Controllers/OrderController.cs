// PART 2 - Step 3: Build OrderController Endpoints
using Microsoft.AspNetCore.Mvc;
using MSFD_LogiTrack.Models;
using MSFD_LogiTrack.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MSFD_LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protects all endpoints in this controller
    public class OrderController : ControllerBase
    {
        private readonly LogiTrackContext _context;


        public OrderController(LogiTrackContext context)
        {
            _context = context;
        }

        // GET: /api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();

            var dtoList = orders.Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                Items = o.Items.Select(i => new ItemDto
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Location = i.Location
                }).ToList()
            }).ToList();

            return dtoList;
        }

        // GET: /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found." });
            }

            var dto = new OrderDto
            {
                OrderId = order.OrderId,
                CustomerName = order.CustomerName,
                Items = order.Items.Select(i => new ItemDto
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Location = i.Location
                }).ToList()
            };

            return dto;
        }

        // POST: /api/orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> PostOrder(OrderDto orderDto)
        {
            if (orderDto.Items == null || !orderDto.Items.Any())
            {
                return BadRequest(new { message = "Order must include at least one item." });
            }

            var order = new Order
            {
                CustomerName = orderDto.CustomerName,
                Items = orderDto.Items.Select(i => new InventoryItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Location = i.Location
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            orderDto.OrderId = order.OrderId; // update DTO with generated ID

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, orderDto);
        }

        // DELETE: /api/orders/{id}
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found." });
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
