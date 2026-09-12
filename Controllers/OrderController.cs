// PART 2 - Step 3: Build OrderController Endpoints
using Microsoft.AspNetCore.Mvc;
using MSFD_LogiTrack.Models;
using MSFD_LogiTrack.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace MSFD_LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protects all endpoints in this controller
    public class OrderController : ControllerBase
    {
        private const string CacheKey = "orders_dto_list";
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;

        public OrderController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /api/order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            if (_cache.TryGetValue(CacheKey, out IEnumerable<OrderDto>? cachedOrders) && cachedOrders != null)
            {
                return Ok(cachedOrders);
            }

            // Direct LINQ projection prevents fetching unnecessary entity columns and redundant mapping loops
            var dtoList = await _context.Orders
                .AsNoTracking()
                .Select(o => new OrderDto
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
                })
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

            _cache.Set(CacheKey, dtoList, cacheOptions);

            return Ok(dtoList);
        }

        // GET: /api/order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var dto = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == id)
                .Select(o => new OrderDto
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
                })
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found." });
            }

            return Ok(dto);
        }

        // POST: /api/order
        [HttpPost]
        public async Task<ActionResult<OrderDto>> PostOrder(OrderDto orderDto)
        {
            // 1. Validate incoming DTO
            if (orderDto == null)
            {
                return BadRequest(new { message = "Order cannot be null." });
            }

            // 2. Validate items
            if (orderDto.Items == null || !orderDto.Items.Any())
            {
                return BadRequest(new { message = "Order must include at least one item." });
            }

            // 3. Map DTO to entity
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

            // 4. Save to database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove(CacheKey);

            // 5. Update DTO with generated ID
            orderDto.OrderId = order.OrderId;

            // 6. Return Created response with DTO
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, orderDto);
        }

        // DELETE: /api/order/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            // 1. Find order by ID
            var order = await _context.Orders
                .Include(o => o.Items)        // eager load items for clean removal
                .FirstOrDefaultAsync(o => o.OrderId == id);

            // 2. Handle not found
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found." });
            }

            // 3. Remove order
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove(CacheKey);

            // 4. Return success (204 No Content)
            return NoContent();
        }
    }
}
