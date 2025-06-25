using Ajimen.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ajimen.Models;

namespace Ajimen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        // 在庫一覧取得
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            return Ok(await _context.Items.ToListAsync());
        }

        // 検索（商品名）
        [HttpGet("search")]
        public async Task<IActionResult> SearchItems([FromQuery] string keyword)
        {
            var result = await _context.Items
                .Where(i => i.ItemName.Contains(keyword))
                .ToListAsync();
            return Ok(result);
        }
    }
}