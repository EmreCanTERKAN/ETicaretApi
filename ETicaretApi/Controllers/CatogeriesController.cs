using ETicaretApi.Data;
using ETicaretApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaretApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatogeriesController : ControllerBase
    {
        private readonly ETicaretContext _context;

        public CatogeriesController(ETicaretContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Category>> GetById (int id)
        {
            var category = _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
            {
                return NotFound();
            }
            return Ok(category);
        }


        [HttpPost]
        public async Task<ActionResult<Category>> Create (Category category)
        {
            _context.Categories.Add(category);
            // En sonda çağırsaydık category eklenmediği için categoryId olmayacaktı bu yüzden patlayacak.
            await _context.SaveChangesAsync();

            var newProduct = new Product
            {
                CategoryId = category.Id,
                Description = "Kategori ekleden geldi",
                Name = "Product 1",
                Price = 10,
                StockQuantity = 10
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);


        }
    }
}
