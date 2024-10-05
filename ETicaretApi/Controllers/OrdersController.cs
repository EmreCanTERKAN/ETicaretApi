using ETicaretApi.Data;
using ETicaretApi.Dto;
using ETicaretApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaretApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ETicaretContext _context;

        public OrdersController(ETicaretContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetById(int id)
        {
            var order = _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        [HttpPost]
        public async Task<ActionResult<Order>> Create (CreateOrderDto order)
        {
            //ilk yöntem  _context.Orders.Add(order);
            using var transaction = await _context.Database.BeginTransactionAsync();
            {
                try
                {
                    var newOrder = new Order
                    {
                        CustomerId = order.CustomerId,
                        OrderDate = DateTime.Now,
                        TotalAmount = 0
                    };

                    _context.Orders.Add(newOrder);
                    await _context.SaveChangesAsync();

                    decimal totalAmount = 0;

                    foreach (var item in order.OrderItems)
                    {
                        var product = await _context.Products.FindAsync(item.productId);
                        if (product is null)
                        {
                            throw new Exception($"{item.productId} idli ürün bulunamadı");
                        }
                        if (product.StockQuantity < item.quantity)
                        {
                            throw new Exception($"{item.productId} yeterli stok yok");
                        }

                        var orderDetail = new OrderDetail
                        {
                            OrderId = newOrder.Id,
                            ProductId = product.Id,
                            Quantity = item.quantity,
                            UnitPrice = product.Price
                        };

                        _context.OrderDetails.Add(orderDetail);
                        totalAmount += orderDetail.Quantity + orderDetail.UnitPrice;

                        product.StockQuantity -= item.quantity;
                        _context.Products.Update(product);

                    }

                    newOrder.TotalAmount = totalAmount;
                    _context.Orders.Update(newOrder);

                    _context.SaveChangesAsync();
                    await transaction.CommitAsync();    
                    return CreatedAtAction(nameof(GetById), new { id = newOrder.Id }, newOrder);
                }
                catch (Exception)
                {

                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
