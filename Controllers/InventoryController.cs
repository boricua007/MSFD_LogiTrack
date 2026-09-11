// PART 2 - Step 2: Build InventoryController Endpoints 

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
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public InventoryController(LogiTrackContext context)
        {
            _context = context;
        }

        // GET: /api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetInventory()
        {
            var items = await _context.InventoryItems.ToListAsync();

            var dtoList = items.Select(i => new ItemDto
            {
                ItemId = i.ItemId,
                Name = i.Name,
                Quantity = i.Quantity,
                Location = i.Location
            }).ToList();

            return dtoList;
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

            return NoContent();
        }
    }
}
