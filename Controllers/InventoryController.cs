// PART 2 - Step 2: Build InventoryController Endpoints 

using Microsoft.AspNetCore.Mvc;
using MSFD_LogiTrack.Models;
using MSFD_LogiTrack.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace MSFD_LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protects all endpoints in this controller
    public class InventoryController : ControllerBase
    {
        private const string CacheKey = "inventory_dto_list";
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;

        public InventoryController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetInventory()
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. Check in-memory cache to avoid repeated DB calls
            if (_cache.TryGetValue(CacheKey, out IEnumerable<ItemDto>? cachedDtoList) && cachedDtoList != null)
            {
                stopwatch.Stop();
                Console.WriteLine($"Cache hit execution time: {stopwatch.ElapsedMilliseconds} ms");
                return Ok(cachedDtoList);
            }

            // 2. Query database using direct projection (Select) & AsNoTracking for optimal query performance
            var dtoList = await _context.InventoryItems
                .AsNoTracking()
                .Select(i => new ItemDto
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Location = i.Location
                })
                .ToListAsync();

            // 3. Store DTO list in cache for 30 seconds
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

            _cache.Set(CacheKey, dtoList, cacheOptions);

            stopwatch.Stop();
            Console.WriteLine($"Cache miss (DB query) execution time: {stopwatch.ElapsedMilliseconds} ms");
            return Ok(dtoList);
        }

        // POST: /api/inventory
        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostInventoryItem(ItemDto itemDto)
        {
            var item = new InventoryItem
            {
                Name = itemDto.Name,
                Quantity = itemDto.Quantity,
                Location = itemDto.Location
            };

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            // Invalidate cache so subsequent GET requests load fresh data
            _cache.Remove(CacheKey);

            itemDto.ItemId = item.ItemId; // update DTO with generated ID

            return CreatedAtAction(nameof(GetInventory), new { id = item.ItemId }, itemDto);
        }

        // DELETE: /api/inventory/{id}
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found." });
            }

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            // Invalidate cache so subsequent GET requests load fresh data
            _cache.Remove(CacheKey);

            return NoContent();
        }
    }
}
