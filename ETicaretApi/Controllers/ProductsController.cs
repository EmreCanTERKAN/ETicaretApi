using ETicaretApi.Data;
using ETicaretApi.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaretApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ETicaretContext _context;

        public ProductsController(ETicaretContext context)
        {
            _context = context;
        }

        [HttpPut("{productName}")]
        public async Task<IActionResult> UpdateProduct(string productName, ProductUpdateDto updateDto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == productName);

            if (product == null)
            {
                return NotFound();
            }
            try
            {
                product.Price *= (1 + (updateDto.PriceIncreasePercentage / 100));
                product.StockQuantity += updateDto.StockIncrease;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Ürün Güncellendi",
                    ProductName = productName,
                    NewPrice = product.Price,
                    NewStockQuantity = product.StockQuantity,
                });
            }
            catch (Exception)
            {

                return StatusCode(500, "Ürün güncellenirken bir hata oluştu");
            }

        }

        [HttpDelete("deleteold")]
        public async Task<IActionResult> DeleteOldOrders([FromQuery] int yearsOld = 1)
        {
            if (yearsOld < 0) return BadRequest();
            var cutoffDate = DateTime.Now.AddDays(-yearsOld);
            int totalDeletedCount = 0;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int batchSize = 1000;
                bool contiuneDeletion = true;
                while (contiuneDeletion)
                {
                    var oldOrders = await _context.Orders.Where(o => o.OrderDate < cutoffDate)
                                                              .Include(o => o.OrderDetails)
                                                              .Take(batchSize)
                                                              .ToListAsync();

                    if (!oldOrders.Any())
                    {
                        contiuneDeletion = false;
                        continue;
                    }

                    foreach (var order in oldOrders)
                    {
                        _context.OrderDetails.RemoveRange(order.OrderDetails);
                    }

                    var deleteCount = await _context.SaveChangesAsync();
                    totalDeletedCount += deleteCount;

                }

                await transaction.CommitAsync();
                return Ok(new {Message = $"{totalDeletedCount} adet order detayları ile birlikte silindi." });


            }
            catch (Exception)
            {

                await transaction.RollbackAsync();
                return BadRequest();
            }
        }

    }


}
